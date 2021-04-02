namespace IdentityServerOidcOpSample.Models
{
    public class OidcClientSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUrl { get; set; }
        public string PostLogoutRedirectUrl { get; set; }
        public string FrontChannelLogoutUri { get; set; }
    }
}
