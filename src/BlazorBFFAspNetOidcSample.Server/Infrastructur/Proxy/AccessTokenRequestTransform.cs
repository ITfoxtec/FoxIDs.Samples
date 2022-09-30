using ITfoxtec.Identity;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Transforms;

namespace BlazorBFFAspNetOidcSample.Server.Infrastructur.Proxy
{
    public class AccessTokenRequestTransform : RequestTransform
    {
        private readonly string accessToken;

        public AccessTokenRequestTransform(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(IdentityConstants.TokenTypes.Bearer, accessToken);
            return default;
        }
    }
}
