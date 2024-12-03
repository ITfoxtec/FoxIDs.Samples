using FoxIDs.SampleHelperLibrary.Models;

namespace AspNetCoreApi2Sample.Models
{
    public class IdentitySettings : LibrarySettings
    {
        public string FoxIDsAuthority { get; set; }

        public string ResourceId => DownParty;

        public string DownParty { get; set; }
    }
}
