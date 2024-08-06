namespace VietnamTrafficPolice.Apis.Supplement;

internal static class TrafficIncidentFormDataExtensions
{
    internal static Dictionary<string, string> ToDictionary(this TrafficIncidentRequest request)
    {
        Dictionary<string, string> dictionary = new();

        var vehicleType = Enum.IsDefined(typeof(VehicleType), request.VehicleType)
            ? (int)request.VehicleType
            : (int)VehicleType.Car;

        dictionary.Add("BienKS", request.PlateId);
        dictionary.Add("Xe", vehicleType.ToString());

        return dictionary;
    }
}