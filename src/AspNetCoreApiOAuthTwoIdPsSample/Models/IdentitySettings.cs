namespace AspNetCoreApiOAuthTwoIdPsSample.Models
{
    public class IdentitySettings
    {
        // IdP 1
        public string Authority1 { get; set; }
        public string ResourceId1 { get; set; }

        // IdP 2
        public string Authority2 { get; set; }
        public string ResourceId2 { get; set; }
    }
}
