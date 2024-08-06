namespace VietnamTrafficPolice.Apis.Supplement;

internal static class TrafficIncidentTableContentExtensions
{
    internal static TrafficIncident ToTrafficIncident(this TrafficIncidentTableContent content)
    {
        return new TrafficIncident(content.LicensePlate,
            content.PlateColor,
            content.VehicleType,
            content.ViolationTime,
            content.ViolationLocation,
            content.ViolationBehavior,
            content.Status,
            content.IssuedBy,
            content.ContactAddress ?? []);
    }
}