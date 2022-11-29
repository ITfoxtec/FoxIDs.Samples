using AspNetCoreApiOAuthTwoIdPsSample.Identity;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Models;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCoreApiOAuthTwoIdPsSample.Policys
{
    public class IdPWithScopesAuthorizationRequirement: AuthorizationHandler<IdPWithScopesAuthorizationRequirement>, IAuthorizationRequirement
    {
        public IdPWithScopesAuthorizationRequirement(IEnumerable<IdPWithScopes> allowedIdpWithScopesList)
        {
            if (allowedIdpWithScopesList?.Any() != true)
            {
                throw new ArgumentNullException(nameof(allowedIdpWithScopesList), "The list of IdP and scopes pairs is null or empty.");
            }

            AllowedIdpWithScopesList = allowedIdpWithScopesList;
        }

        /// <summary>
        /// List of IdP and scopes pairs. One or more IdP and scope link must match.
        /// </summary>
        public IEnumerable<IdPWithScopes> AllowedIdpWithScopesList { get; }

        /// <summary>
        /// Makes a decision if authorization is allowed based on the IdP and scope list requirements specified.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IdPWithScopesAuthorizationRequirement requirement)
        {
            if (context.User != null)
            {
                var idpClaimValue = context.User.Claims.Where(c => string.Equals(c.Type, ApiJwtClaimTypes.IdP, StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).FirstOrDefault();
                if (!idpClaimValue.IsNullOrWhiteSpace())
                {
                    var idpScopesItem = requirement.AllowedIdpWithScopesList.Where(sr => idpClaimValue.Equals(sr.IdP, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                    if (idpScopesItem != null)
                    {
                        if (idpScopesItem.Scopes?.Any() != true)
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            var scopeClaimValue = context.User.Claims.Where(c => string.Equals(c.Type, JwtClaimTypes.Scope, StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).FirstOrDefault();
                            if (!scopeClaimValue.IsNullOrWhiteSpace())
                            {
                                var scopes = scopeClaimValue.ToSpaceList();
                                foreach (var scope in scopes)
                                {
                                    if (idpScopesItem.Scopes.Contains(scope, StringComparer.Ordinal))
                                    {
                                        context.Succeed(requirement);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
