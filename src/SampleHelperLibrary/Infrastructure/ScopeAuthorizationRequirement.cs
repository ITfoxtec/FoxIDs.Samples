using ITfoxtec.Identity;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleHelperLibrary.Infrastructure
{
    public class ScopeAuthorizationRequirement : AuthorizationHandler<ScopeAuthorizationRequirement>, IAuthorizationRequirement
    {
        /// <summary>
        /// Creates a new instance of <see cref="ClaimsAuthorizationRequirement"/>.
        /// </summary>
        /// <param name="allowedValues">The list of scope values the scope must match one or more of.</param>
        public ScopeAuthorizationRequirement(IEnumerable<string> allowedValues)
        {
            if (allowedValues == null || !allowedValues.Any())
            {
                throw new ArgumentNullException(nameof(allowedValues), "The list of scope values is null or empty.");
            }

            AllowedValues = allowedValues;
        }

        /// <summary>
        /// Gets the list of scope values the scope must match one or more of.
        /// </summary>
        public IEnumerable<string> AllowedValues { get; }

        /// <summary>
        /// Makes a decision if authorization is allowed based on the scopes requirements specified.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeAuthorizationRequirement requirement)
        {
            if (context.User != null)
            {
                var scopeClaimValue = context.User.Claims.Where(c => string.Equals(c.Type, JwtClaimTypes.Scope, StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).FirstOrDefault();
                if(!scopeClaimValue.IsNullOrWhiteSpace())
                {
                    var scopes = scopeClaimValue.ToSpaceList();
                    foreach(var scope in scopes)
                    {
                        if (requirement.AllowedValues.Contains(scope, StringComparer.Ordinal))
                        {
                            context.Succeed(requirement);
                            break;
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
