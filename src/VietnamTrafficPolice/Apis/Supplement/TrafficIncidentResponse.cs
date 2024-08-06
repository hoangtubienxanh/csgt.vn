namespace VietnamTrafficPolice.Apis.Supplement;

public record TrafficIncidentResponse(
    bool Succeeded,
    DateTime ServerTime,
    ICollection<TrafficIncident>? TrafficViolations);