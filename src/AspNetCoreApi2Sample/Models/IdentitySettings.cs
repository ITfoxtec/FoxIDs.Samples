namespace AspNetCoreApi2Sample.Models
{
    public class IdentitySettings
    {
        public string FoxIDsAuthority { get; set; }

        public string ResourceId => DownParty;

        public string DownParty { get; set; }
    }
}
