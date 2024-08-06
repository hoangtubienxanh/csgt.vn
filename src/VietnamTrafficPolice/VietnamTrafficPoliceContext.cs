using System.Text.Json;
using System.Text.Json.Serialization;

using VietnamTrafficPolice.Apis;

namespace VietnamTrafficPolice;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(TrafficIncidentTableContent))]
[JsonSerializable(typeof(QueryingResponse))]
internal partial class VietnamTrafficPoliceContext : JsonSerializerContext;