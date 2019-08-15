using Microsoft.AspNetCore.Authorization;
using SampleHelperLibrary.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SampleHelperLibrary
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="ScopeAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="allowedValues">Values the scope must process one or more of for evaluation to succeed.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder policy, params string[] allowedValues)
        {
            return RequireScope(policy, (IEnumerable<string>)allowedValues);

        }

        /// <summary>
        /// Adds a <see cref="ScopeAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="allowedValues">Values the scope must process one or more of for evaluation to succeed.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder policy, IEnumerable<string> allowedValues)
        {
            if (allowedValues == null || !allowedValues.Any())
            {
                throw new ArgumentNullException(nameof(allowedValues), "The list of scope values is null or empty.");
            }

            policy.Requirements.Add(new ScopeAuthorizationRequirement(allowedValues));
            return policy;
        }
    }
}
