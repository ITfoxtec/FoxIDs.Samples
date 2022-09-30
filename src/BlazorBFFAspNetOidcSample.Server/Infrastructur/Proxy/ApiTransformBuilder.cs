using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace BlazorBFFAspNetOidcSample.Server.Infrastructur.Proxy
{
    public class ApiTransformBuilder
    {
        private readonly ITransformBuilder transformBuilder;

        public ApiTransformBuilder(ITransformBuilder transformBuilder)
        {
            this.transformBuilder = transformBuilder;
        }

        public HttpTransformer GetHttpTransformer(HttpContext httpContext, string proxyApiPath)
        {
            return transformBuilder.Create(async context =>
            {
                context.UseDefaultForwarders = true;
                context.CopyRequestHeaders = true;

                var accessToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    context.RequestTransforms.Add(new AccessTokenRequestTransform(accessToken));
                }
                context.RequestTransforms.Add(new RequestHeaderRemoveTransform("Cookie"));
                context.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, proxyApiPath));
            });
        }
    }
}
