namespace AspNetCoreSamlSample.Models
{
    public class Settings
    {
        public string DownParty { get; set; }
        public string FoxIDsLoginUpParty { get; set; }
        public string ParallelFoxIDsUpParty { get; set; }
        public string IdentityServerUpParty { get; set; }
        public string SamlIdPSampleUpParty { get; set; }
        public string SamlIdPAdfsUpParty { get; set; }


        public string TokenExchangeDownParty { get; set; }
        public string TokenExchangeClientId => DownParty;
        public string TokenExchangeEndpoint { get; set; }
        public string TokenExchangeClientCertificateFile { get; set; }
        public string TokenExchangeClientCertificatePassword { get; set; }

        public string AspNetCoreApi1SampleUrl { get; set; }
    }
}
