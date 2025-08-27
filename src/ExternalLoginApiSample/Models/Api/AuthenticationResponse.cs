namespace ExternalLoginApiSample.Models.Api
{
    public class AuthenticationResponse
    {
        public IEnumerable<ClaimValue> Claims { get; set; }
    }
}
