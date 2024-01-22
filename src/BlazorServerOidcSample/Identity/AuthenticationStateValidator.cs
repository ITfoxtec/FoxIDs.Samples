using FoxIDs.SampleHelperLibrary.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using ITfoxtec.Identity;

namespace BlazorServerOidcSample.Identity
{
    public class AuthenticationStateValidator : RevalidatingServerAuthenticationStateProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LogoutMemoryCache logoutMemoryCache;

        public AuthenticationStateValidator(ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor, LogoutMemoryCache logoutMemoryCache) : base(loggerFactory) 
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logoutMemoryCache = logoutMemoryCache;
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(5);

        protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            var sessionId = httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == JwtClaimTypes.SessionId).Select(c => c.Value).FirstOrDefault();
            foreach (var item in logoutMemoryCache.List)
            {
                if (sessionId == item)
                {
                    return Task.FromResult(false);
                }
            }
            return Task.FromResult(true);
        }
    }
}
