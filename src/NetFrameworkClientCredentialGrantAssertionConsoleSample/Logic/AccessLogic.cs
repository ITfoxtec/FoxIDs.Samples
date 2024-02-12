using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using ITfoxtec.Identity.Saml2.Util;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using NetFrameworkClientCredentialGrantAssertionConsoleSample.Models;
using System.IdentityModel.Tokens;

namespace NetFrameworkClientCredentialGrantAssertionConsoleSample.Logic
{
    public static class AccessLogic
    {
        public static async Task<string> GetAccessTokenAsync(HttpClient httpClient, string scope)
        {
            Console.WriteLine("Acquire access token...");

            var authority = ConfigurationManager.AppSettings["FoxIDsAuthority"];
            var tokenEndpoint = $"{authority}oauth/token";

            var clientId = ConfigurationManager.AppSettings["DownParty"];

            var clientAssertion = GetClientAssertionToken(tokenEndpoint, clientId);

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            var nameValueCollection = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                { "client_assertion", clientAssertion },
                { "grant_type", "client_credentials" },
                { "scope", scope }
            };
            request.Content = new FormUrlEncodedContent(nameValueCollection);

            using (var response = await httpClient.SendAsync(request))
            {
                try
                {
                    var result = await response.Content.ReadAsStringAsync();

                    var tokenResponse = Deserialize<TokenResponse>(result);

                    if (!string.IsNullOrEmpty(tokenResponse.error))
                    {
                        throw new Exception($"{tokenResponse.error}, {tokenResponse.error_description}");
                    }

                    if (string.IsNullOrEmpty(tokenResponse.access_token))
                    {
                        throw new ArgumentNullException("access_token");
                    }

                    if (string.IsNullOrEmpty(tokenResponse.token_type))
                    {
                        throw new ArgumentNullException("token_type");
                    }

                    Console.WriteLine($"Access token: {tokenResponse.access_token.Substring(0, 40)}...");
                    return tokenResponse.access_token;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error execute token request with client credentials assertion. StatusCode={response.StatusCode}", ex);
                }
            }

        }

        public static string GetClientAssertionToken(string tokenEndpoint, string clientId)
        {
            var clientCertificateFile = ConfigurationManager.AppSettings["ClientCertificateFile"];
            var clientCertificatePassword = ConfigurationManager.AppSettings["ClientCertificatePassword"];
            var clientCertificate = CertificateUtil.Load(clientCertificateFile, clientCertificatePassword);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Expires = DateTime.Now.AddMinutes(Convert.ToInt32(5)),
                SigningCredentials = new X509SigningCredentials(clientCertificate, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest),
                //Issuer = clientId,
                Subject = new ClaimsIdentity(new List<Claim> { new Claim("sub", clientId) }),
                //Audience = tokenEndpoint,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = (JwtSecurityToken)tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static T Deserialize<T>(string Json)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Json)))
            {
                var serialiser = new DataContractJsonSerializer(typeof(T));
                var deserializedObj = (T)serialiser.ReadObject(ms);
                return deserializedObj;
            }
        }
    }
}
