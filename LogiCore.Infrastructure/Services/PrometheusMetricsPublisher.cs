using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using LogiCore.Application.Common.Interfaces;
using Prometheus;

namespace LogiCore.Infrastructure.Services
{
    /// <summary>
    /// Background service that periodically queries IMetricsService and updates Prometheus Gauges
    /// so business metrics are available to Prometheus/Grafana.
    /// Interval configurable via `Metrics:PublishIntervalSeconds` (default 30s).
    /// </summary>
    public class PrometheusMetricsPublisher : BackgroundService
    {
        private readonly IMetricsService _metricsService;
        private readonly TimeSpan _interval;

        private readonly Gauge _totalPackagesGauge;
        private readonly Gauge _totalShipmentsGauge;
        private readonly Gauge _deliveredPackagesGauge;
        private readonly Gauge _inTransitPackagesGauge;
        private readonly Gauge _canceledPackagesGauge;
        private readonly Gauge _avgDispatchToDeliveryMinutesGauge;

        public PrometheusMetricsPublisher(IMetricsService metricsService, IConfiguration configuration)
        {
            _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));

            var seconds = 30;
            if (int.TryParse(configuration["Metrics:PublishIntervalSeconds"], out var s) && s > 0)
                seconds = s;

            _interval = TimeSpan.FromSeconds(seconds);

            _totalPackagesGauge = Metrics.CreateGauge("logicore_total_packages", "Total de paquetes en el sistema");
            _totalShipmentsGauge = Metrics.CreateGauge("logicore_total_shipments", "Total de envíos en el sistema");
            _deliveredPackagesGauge = Metrics.CreateGauge("logicore_packages_delivered", "Paquetes entregados");
            _inTransitPackagesGauge = Metrics.CreateGauge("logicore_packages_in_transit", "Paquetes en tránsito");
            _canceledPackagesGauge = Metrics.CreateGauge("logicore_packages_canceled", "Paquetes cancelados");
            _avgDispatchToDeliveryMinutesGauge = Metrics.CreateGauge("logicore_avg_dispatch_to_delivery_minutes", "Promedio minutos entre dispatch y delivery");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var overview = await _metricsService.GetOverviewAsync();

                    if (overview != null)
                    {
                        if (overview.TryGetValue("totalPackages", out var tp)) _totalPackagesGauge.Set(ToDouble(tp));
                        if (overview.TryGetValue("totalShipments", out var ts)) _totalShipmentsGauge.Set(ToDouble(ts));
                        if (overview.TryGetValue("delivered", out var d)) _deliveredPackagesGauge.Set(ToDouble(d));
                        if (overview.TryGetValue("inTransit", out var it)) _inTransitPackagesGauge.Set(ToDouble(it));
                        if (overview.TryGetValue("canceled", out var c)) _canceledPackagesGauge.Set(ToDouble(c));
                    }

                    var avg = await _metricsService.GetAverageDispatchToDeliveryAsync();
                    if (avg.HasValue) _avgDispatchToDeliveryMinutesGauge.Set(avg.Value.TotalMinutes);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PrometheusMetricsPublisher] Error updating metrics: {ex.Message}");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException) { }
            }
        }

        private static double ToDouble(object? v)
        {
            if (v is null) return 0;
            return Convert.ToDouble(v);
        }
    }
}
