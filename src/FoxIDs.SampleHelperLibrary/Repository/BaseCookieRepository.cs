using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FoxIDs.SampleHelperLibrary.Repository
{
    public abstract class BaseCookieRepository
    {
        private readonly IDataProtectionProvider dataProtection;
        private readonly IHttpContextAccessor httpContextAccessor;

        public BaseCookieRepository(IDataProtectionProvider dataProtection, IHttpContextAccessor httpContextAccessor)
        {
            this.dataProtection = dataProtection;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected Task<string> GetValueAsync()
        {
            var cookieName = CookieName();
            var cookie = httpContextAccessor.HttpContext.Request.Cookies[cookieName];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                try
                {
                    return Task.FromResult(Unprotect(cookie));
                }
                catch (CryptographicException ex)
                {
                    throw new Exception($"Unable to Unprotect Cookie '{cookieName}'.", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to Read Cookie '{cookieName}'.", ex);
                }
            }
            else
            {
                return Task.FromResult(string.Empty);
            }
        }

        protected Task SaveValueAsync(string value)
        {
            var cookieOptions = new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                IsEssential = true,
            };

            httpContextAccessor.HttpContext.Response.Cookies.Append(
                CookieName(),
                Protect(value),
                cookieOptions);
            return Task.FromResult(0);
        }

        protected Task DeleteValueAsync()
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(
                CookieName(),
                string.Empty,
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMonths(-1),
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    IsEssential = true,
                });
            return Task.FromResult(0);
        }

        private IDataProtector CreateProtector()
        {
            return dataProtection.CreateProtector(new[] { CookieName() });
        }

        private string Protect(string content)
        {
            return CreateProtector().Protect(content);
        }

        private string Unprotect(string content)
        {
            return CreateProtector().Unprotect(content);
        }

        private string CookieName()
        {
            return $"{httpContextAccessor.HttpContext.Request.Host}.{PostCookieName}";
        }

        protected abstract string PostCookieName { get; }
    }
}
