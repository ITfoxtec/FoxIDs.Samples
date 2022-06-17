using UrlCombineLib;

namespace FoxIDs.SampleHelperLibrary.Models
{
    public class FoxIDsSettings
    {
        public string Authority => FoxIDsEndpoint == null ? null : (IncludeTenantInUrl ? UrlCombine.Combine(FoxIDsEndpoint, Tenant, Track, DownParty) : UrlCombine.Combine(FoxIDsEndpoint, Track, DownParty));

        public string FoxIDsEndpoint { get; set; }
        public bool IncludeTenantInUrl { get; set; }
        public string Tenant { get; set; }
        public string Track { get; set; }
        public string DownParty { get; set; }
    }
}
