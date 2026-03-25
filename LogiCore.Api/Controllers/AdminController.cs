using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace LogiCore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IMetricsService _metrics;

        public AdminController(IMetricsService metrics)
        {
            _metrics = metrics;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> Overview()
        {
            var data = await _metrics.GetOverviewAsync();
            return Ok(data);
        }

        [HttpGet("shipments-per-day")]
        public async Task<IActionResult> ShipmentsPerDay([FromQuery] int days = 30)
        {
            var data = await _metrics.GetShipmentsPerDayAsync(days);
            return Ok(data);
        }

        [HttpGet("avg-dispatch-to-delivery")]
        public async Task<IActionResult> AvgDispatchToDelivery()
        {
            var data = await _metrics.GetAverageDispatchToDeliveryAsync();
            return Ok(new { average = data?.TotalMinutes });
        }
    }
}
