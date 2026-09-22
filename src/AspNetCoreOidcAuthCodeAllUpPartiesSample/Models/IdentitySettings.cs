using FoxIDs.SampleHelperLibrary.Models;

namespace AspNetCoreOidcAuthCodeAllUpPartiesSample.Models
{
    public class IdentitySettings : LibrarySettings
    {
        public string FoxIDsAuthority { get; set; }
        public string ClientId => DownParty;
        public string DownParty { get; set; }
        public string ClientSecret { get; set; }

        public bool SendLoginParameters { get; set; }
        public string LoginParameterProfileId { get; set; }
        public string LoginParameterDepartments { get; set; }

        public bool SendImpersonationParameter { get; set; }
        public string ImpersonatedUserId { get; set; }

        public bool IncludeApiScope { get; set; }

        public string RequestApi1Scope { get; set; }
        public string RequestApi2Scope { get; set; }
    }
}
