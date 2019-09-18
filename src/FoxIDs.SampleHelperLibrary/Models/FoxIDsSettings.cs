using UrlCombineLib;

namespace FoxIDs.SampleHelperLibrary.Models
{
    public class FoxIDsSettings
    {
        public string Authority => FoxIDsEndpoint == null ? null : UrlCombine.Combine(FoxIDsEndpoint, Tenant, Track, DownParty);

        public string FoxIDsEndpoint { get; set; }
        public string Tenant { get; set; }
        public string Track { get; set; }
        public string DownParty { get; set; }
    }
}
