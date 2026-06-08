namespace AspNetCoreWsFedSample.Models;

public class WsFederationSettings
{
    public string MetadataAddress { get; set; } = "https://localhost:44330/test-corp/test/aspnetcore-wsfed-sample(*)/wsfed/stsmetadata";

    public string Wtrealm { get; set; } = "https://localhost:44358/";

    public string Wreply { get; set; } = "https://localhost:44358/signin-wsfed";

    public string SignOutWreply { get; set; } = "https://localhost:44358/";

    public string CallbackPath { get; set; } = "/signin-wsfed";

    public string RemoteSignOutPath { get; set; } = "/signout-wsfed";

    public bool RequireHttpsMetadata { get; set; } = false;
}
