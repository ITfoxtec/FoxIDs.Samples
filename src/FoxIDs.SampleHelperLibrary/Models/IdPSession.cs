namespace FoxIDs.SampleHelperLibrary.Models
{
    public class IdPSession
    {
        public string RelyingPartyIssuer { get; set; }
        public string NameIdentifier { get; set; }
        public string Upn { get; set; }
        public string Email { get; set; }
        public string CustomId { get; set; }
        public string CustomName { get; set; }
        public string SessionIndex { get; set; }
    }
}
