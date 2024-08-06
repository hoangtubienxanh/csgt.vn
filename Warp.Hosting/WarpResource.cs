namespace Warp.Hosting;

public class WarpResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    /// <summary>
    ///     Gets the primary endpoint for the Cloudflare WARP server.
    /// </summary>
    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new EndpointReference(this, PrimaryEndpointName);

    /// <summary>
    ///     Gets the connection string expression for the Cloudflare WARP server.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{PrimaryEndpoint.Property(EndpointProperty.Url)}");
}