using VietnamTrafficPolice.Apis.Supplement;

namespace VietnamTrafficPolice;

public interface IVietnamTrafficPoliceClient
{
    string TrafficIncidentCaptchaSolution { get; }

    Task<TrafficIncidentResponse> GetTrafficIncidentById(TrafficIncidentRequest request);

    Task<TrafficIncidentResponse> GetTrafficIncidentById(TrafficIncidentRequest request,
        string trafficIncidentCaptchaSolution);

    Task<(byte[], string)> SolveTrafficIncidentCaptchaChallenge(Func<byte[], Task<string>> onChallengeReceived);
}