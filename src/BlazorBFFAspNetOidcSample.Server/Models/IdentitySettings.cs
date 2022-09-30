using FoxIDs.SampleHelperLibrary.Models;

namespace BlazorBFFAspNetOidcSample.Server.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientSecret { get; set; }
    }
}
