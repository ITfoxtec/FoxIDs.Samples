using FoxIDs.SampleHelperLibrary.Models;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace FoxIDs.SampleHelperLibrary.Infrastructure.Hosting
{
    public class ProxyHeadersMiddleware
    {
        protected readonly RequestDelegate next;

        public ProxyHeadersMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public virtual async Task Invoke(HttpContext context)
        {
            if (!(IsHealthCheck(context) || IsLoopback(context)))
            {
                ReadSchemeFromHeader(context);
            }

            await next.Invoke(context);
        }

        protected virtual bool IsHealthCheck(HttpContext context)
        {
            if (context.Request?.Path == "/" || $"/api/health".Equals(context.Request?.Path, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        protected bool IsLoopback(HttpContext context)
        {
            if (context.Connection?.RemoteIpAddress != null)
            {
                return IPAddress.IsLoopback(context.Connection.RemoteIpAddress);
            }
            return false;
        }

        protected void ReadSchemeFromHeader(HttpContext context)
        {
            var settings = context.RequestServices.GetService<LibrarySettings>();
            if (settings.TrustProxySchemeHeader)
            {
                string schemeHeader = context.Request.Headers["X-Forwarded-Scheme"];
                if (schemeHeader.IsNullOrWhiteSpace())
                {
                    schemeHeader = context.Request.Headers["X-Forwarded-Proto"];
                }
                if (!schemeHeader.IsNullOrWhiteSpace())
                {
                    if (schemeHeader.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Scheme = Uri.UriSchemeHttp;
                    }
                    else if (schemeHeader.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Scheme = Uri.UriSchemeHttps;
                    }
                }
            }
        }
    }
}
