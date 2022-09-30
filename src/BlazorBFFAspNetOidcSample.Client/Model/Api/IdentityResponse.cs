using System.Collections.Generic;

namespace BlazorBFFAspNetOidcSample.Models.Api
{
    public class IdentityResponse
    {
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public IEnumerable<ClaimValue> Claims { get; set; }
    }
}
