using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FoxIDs.SampleHelperLibrary.Repository
{
    public class IdPSelectionCookieRepository
    {
        private readonly IDataProtectionProvider dataProtection;
        private readonly IHttpContextAccessor httpContextAccessor;

        public IdPSelectionCookieRepository(IDataProtectionProvider dataProtection, IHttpContextAccessor httpContextAccessor)
        {
            this.dataProtection = dataProtection;
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<string> GetAsync()
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
                return null;
            }
        }

        public Task SaveAsync(string idp)
        {
            var cookieOptions = new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
            };

            httpContextAccessor.HttpContext.Response.Cookies.Append(
                CookieName(),
                Protect(idp),
                cookieOptions);
            return Task.FromResult(0);
        }

        public Task DeleteAsync()
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(
                CookieName(),
                string.Empty,
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMonths(-1),
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
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
            return $"{httpContextAccessor.HttpContext.Request.Host}.selected.idp";
        }

    }
}
