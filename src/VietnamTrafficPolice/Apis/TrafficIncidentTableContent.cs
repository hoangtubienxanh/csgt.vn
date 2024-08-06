using System.Text.Json.Serialization;

namespace VietnamTrafficPolice.Apis;

internal record TrafficIncidentTableContent(
    [property: JsonPropertyName("Biển kiểm soát:")]
    string LicensePlate,
    [property: JsonPropertyName("Màu biển:")]
    string? PlateColor,
    [property: JsonPropertyName("Loại phương tiện:")]
    string? VehicleType,
    [property: JsonPropertyName("Thời gian vi phạm: ")]
    string? ViolationTime,
    [property: JsonPropertyName("Địa điểm vi phạm:")]
    string? ViolationLocation,
    [property: JsonPropertyName("Hành vi vi phạm:")]
    string? ViolationBehavior,
    [property: JsonPropertyName("Trạng thái: ")]
    string? Status,
    [property: JsonPropertyName("Đơn vị phát hiện vi phạm: ")]
    string? IssuedBy,
    [property: JsonPropertyName("Nơi giải quyết vụ việc: ")]
    List<string>? ContactAddress
);