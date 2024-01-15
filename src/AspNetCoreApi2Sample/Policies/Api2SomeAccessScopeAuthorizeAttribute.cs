using ITfoxtec.Identity;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCoreApi2Sample.Policies
{
    public class Api2SomeAccessScopeAuthorizeAttribute : AuthorizeAttribute
    {
        public const string Name = nameof(Api2SomeAccessScopeAuthorizeAttribute);

        public Api2SomeAccessScopeAuthorizeAttribute() : base(Name)
        { }

        public static void AddPolicy(AuthorizationOptions options)
        {
            options.AddPolicy(Name, policy =>
            {
                policy.RequireScope("aspnetcore_api2_sample:some_2_access");
            });
        }
    }
}
