using FoxIDs.SampleHelperLibrary.Models;

namespace AspNetCoreApi1Sample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ResourceId => DownParty;
    }
}
