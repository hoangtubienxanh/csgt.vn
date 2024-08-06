namespace VietnamTrafficPolice.Apis.Supplement;

public record TrafficIncident(
    string FormattedLicensePlate,
    string? Attributes,
    string? VehicleType,
    string? OccurredAt,
    string? PartialAddress,
    string? Reason,
    string? Status,
    string? IssuedBy,
    List<string> ContactInformation);