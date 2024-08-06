using Warp.Hosting.Utils;

namespace Warp.Hosting;

public static class WarpBuilderExtensions
{
    private const string LicenseEnvVarName = "WARP_LICENSE_KEY";

    // The path within the container in which WARP stores its data
    private const string WarpContainerDataDirectory = "/var/lib/cloudflare-warp";

    /// <summary>
    ///     Adds a Cloudflare WARP resource to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder" />.</param>
    /// <param name="name">The name to give the resource.</param>
    /// <param name="port">The host port for the Seq server.</param>
    /// <param name="license">
    ///     The license key of the WARP client.
    ///     If you have subscribed to WARP+ service, you can fill in the key in this environment variable.
    ///     If you have not subscribed to WARP+ service, you can ignore this environment variable.
    /// </param>
    public static IResourceBuilder<WarpResource> AddWarp(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<ParameterResource>? license = null,
        int? port = null)
    {
        var warpResource = new WarpResource(name);

        var resourceBuilder = builder.AddResource(warpResource)
            .WithHttpEndpoint(port, 1080, WarpResource.PrimaryEndpointName)
            .WithImage(WarpContainerImageTargs.Image)
            .WithImageRegistry(WarpContainerImageTargs.Registry)
            .WithEnvironment(context =>
            {
                if (license?.Resource is { } licenseKey)
                {
                    context.EnvironmentVariables[LicenseEnvVarName] = licenseKey;
                }
            })
            .WithContainerRuntimeArgs(
                "--cap-add=NET_ADMIN",
                "--sysctl=net.ipv6.conf.all.disable_ipv6=0",
                "--sysctl=net.ipv4.conf.all.src_valid_mark=1");

        return resourceBuilder;
    }

    /// <summary>
    ///     Adds a named volume for the data folder to a WARP container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">
    ///     The name of the volume. Defaults to an auto-generated name based on the application and resource
    ///     names.
    /// </param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}" />.</returns>
    public static IResourceBuilder<WarpResource> WithDataVolume(this IResourceBuilder<WarpResource> builder,
        string? name = null, bool isReadOnly = false)
    {
        return builder.WithVolume(name ?? VolumeNameGenerator.CreateVolumeName(builder, "data"),
            WarpContainerDataDirectory,
            isReadOnly);
    }

    /// <summary>
    ///     Adds a bind mount for the data folder to a WARP container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}" />.</returns>
    public static IResourceBuilder<WarpResource> WithDataBindMount(this IResourceBuilder<WarpResource> builder,
        string source, bool isReadOnly = false)
    {
        return builder.WithBindMount(source, WarpContainerDataDirectory, isReadOnly);
    }
}