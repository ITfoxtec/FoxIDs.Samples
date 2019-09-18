using ITfoxtec.Identity;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCoreApi1Sample.Policys
{
    public class Api1SomeAccessScopeAuthorizeAttribute : AuthorizeAttribute
    {
        public const string Name = nameof(Api1SomeAccessScopeAuthorizeAttribute);

        public Api1SomeAccessScopeAuthorizeAttribute() : base(Name)
        { }

        public static void AddPolicy(AuthorizationOptions options)
        {
            options.AddPolicy(Name, policy =>
            {
                policy.RequireScope("aspnetcore_api1_sample:some_access");
            });
        }
    }
}
