using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using BlazorBFFAspNetOidcSample.Server.Infrastructur.Proxy;
using System.Net.Http;
using System;
using Yarp.ReverseProxy.Forwarder;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using System.Collections.Concurrent;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;

namespace BlazorBFFAspNetOidcSample.Server.Infrastructur
{
    public static class MvcCoreEndpointBuilderExtensions
    {
        private static readonly ConcurrentDictionary<string, HttpMessageInvoker> httpMessageInvokers = new();

        public static IEndpointConventionBuilder AddApiProxy(this IEndpointRouteBuilder endpoints, string proxyApiPath, string remoteApiUrl, int proxyTimeoutInSeconds = 100)
        {
            return endpoints.Map(new PathString(proxyApiPath).Add("/{**catchall}").Value, async httpContext =>
            {
                try
                {
                    if (!httpContext.User.Identity.IsAuthenticated)
                    {
                        httpContext.Response.StatusCode = 401;

                        throw new AuthenticationException("User is not authenticated.");
                    }

                    var httpMessageInvoker = httpMessageInvokers.GetOrAdd(proxyApiPath, (key) =>
                    {
                        return new HttpMessageInvoker(new SocketsHttpHandler()
                        {
                            UseProxy = false,
                            AllowAutoRedirect = false,
                            AutomaticDecompression = DecompressionMethods.None,
                            UseCookies = false,
                            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current)
                        });
                    });

                    var antiforgery = httpContext.RequestServices.GetRequiredService<IAntiforgery>();
                    await antiforgery.ValidateRequestAsync(httpContext);

                    var httpForwarder = httpContext.RequestServices.GetRequiredService<IHttpForwarder>();
                    var apiTransformBuilder = httpContext.RequestServices.GetRequiredService<ApiTransformBuilder>();

                    var requestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(proxyTimeoutInSeconds) };
                    var error = await httpForwarder.SendAsync(httpContext, remoteApiUrl, httpMessageInvoker, requestConfig, apiTransformBuilder.GetHttpTransformer(httpContext, proxyApiPath));
                    if (error != ForwarderError.None)
                    {
                        var errorFeature = httpContext.GetForwarderErrorFeature();
                        httpContext.Response.StatusCode = 500;
                        throw new Exception($"Proxy error. {errorFeature.Error}.", errorFeature.Exception);
                    }
                }
                catch (Exception ex)
                {
                    var logger = httpContext.RequestServices.GetRequiredService<ILogger<IHttpForwarder>>();
                    logger.LogError($"Proxy error, proxy API path '{proxyApiPath}'.", ex);
                    throw;
                }
            });
        }
    }
}
