namespace LogiCore.Domain.Entities;

public enum PackageStatus
{
    Pending = 0,
    InTransit = 1,
    Delivered = 2,
    Canceled = 3,
    AtDepot = 4,
    DeliveredToCenter = 5,
    Returned = 6,
    Collected = 7,
    LastMile = 8
}
