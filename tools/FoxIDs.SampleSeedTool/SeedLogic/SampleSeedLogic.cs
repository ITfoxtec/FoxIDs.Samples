using FoxIDs.SampleSeedTool.Model;
using FoxIDs.SampleSeedTool.ServiceAccess;
using FoxIDs.SampleSeedTool.ServiceAccess.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace FoxIDs.SampleSeedTool.SeedLogic
{
    public class SampleSeedLogic
    {
        const string loginName = "login";

        const string aspNetCoreSamlIdPSampleUpPartyName = "aspnetcore_saml_idp_sample";

        const string aspNetCoreApi1DownPartyName = "aspnetcore_api1_sample";
        const string aspNetCoreOidcAuthCodeDownPartyName = "aspnetcore_oidcauthcode_sample";

        private readonly SeedSettings settings;
        private readonly FoxIDsApiClient foxIDsApiClient;

        public SampleSeedLogic(SeedSettings settings, FoxIDsApiClient foxIDsApiClient)
        {
            this.settings = settings;
            this.foxIDsApiClient = foxIDsApiClient;
        }

        public async Task SeedAsync()
        {
            Console.WriteLine("Create sample configuration");

            await CreateLoginUpPartyIfNotExistsAsync();

            await CreateAspNetCoreSamlIdPSampleUpPartyAsync();

            await CreateAspNetCoreApi1SampleDownPartyAsync();
            await CreateAspNetCoreOidcAuthCodeSampleDownPartyAsync();

            Console.WriteLine(string.Empty);
            Console.WriteLine($"Sample configuration created");
        }

        public async Task DeleteAsync()
        {
            Console.WriteLine("Delete sample configuration");

            Console.WriteLine("Delete Oidc down party sample configuration");
            var oidcDownPartyNames = new[] { aspNetCoreOidcAuthCodeDownPartyName };
            foreach(var name in oidcDownPartyNames)
            {
                try
                {
                    await foxIDsApiClient.DeleteOidcDownPartyAsync(name);
                    Console.WriteLine($"'{name}' configuration deleted");
                }
                catch (FoxIDsApiException ex)
                {
                    if(ex.StatusCode == StatusCodes.Status404NotFound)
                    {
                        Console.WriteLine($"'{name}' configuration not found");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            Console.WriteLine("Delete OAuth down party sample configuration");
            var oauthDownPartyNames = new[] { aspNetCoreApi1DownPartyName };
            foreach (var name in oauthDownPartyNames)
            {
                try
                {
                    await foxIDsApiClient.DeleteOAuthDownPartyAsync(name);
                    Console.WriteLine($"'{name}' configuration deleted");
                }
                catch (FoxIDsApiException ex)
                {
                    if (ex.StatusCode == StatusCodes.Status404NotFound)
                    {
                        Console.WriteLine($"'{name}' configuration not found");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            Console.WriteLine("Delete SAML up party sample configuration");
            var samlUpPartyNames = new[] { aspNetCoreSamlIdPSampleUpPartyName };
            foreach (var name in samlUpPartyNames)
            {
                try
                {
                    await foxIDsApiClient.DeleteSamlUpPartyAsync(name);
                    Console.WriteLine($"'{name}' configuration deleted");
                }
                catch (FoxIDsApiException ex)
                {
                    if (ex.StatusCode == StatusCodes.Status404NotFound)
                    {
                        Console.WriteLine($"'{name}' configuration not found");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine($"Sample configuration deleted");
        }

        private async Task CreateLoginUpPartyIfNotExistsAsync()
        {
            var name = loginName;
            Console.WriteLine($"Check if '{name}' exists");

            try
            {
                _ = await foxIDsApiClient.GetLoginUpPartyAsync(name);

                Console.WriteLine($"'{name}' exists");
            }
            catch (FoxIDsApiException ex)
            {
                if (ex.StatusCode == StatusCodes.Status404NotFound)
                {
                    Console.WriteLine($"Do not exist, creating '{name}'");

                    var loginUpParty = new LoginUpParty
                    {
                        Name = name,
                        SessionLifetime = 3600,
                        SessionAbsoluteLifetime = 43200,
                        EnableCancelLogin = true,
                        EnableCreateUser = true,
                        LogoutConsent = LoginUpPartyLogoutConsent.IfRequered,
                        AllowIframeOnDomains = new[] { "localhost:44344", "*.localhost:44" } 
                    };

                    await foxIDsApiClient.PostLoginUpPartyAsync(loginUpParty);

                    Console.WriteLine($"'{name}' created");
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateAspNetCoreSamlIdPSampleUpPartyAsync()
        {
            var name = aspNetCoreSamlIdPSampleUpPartyName;
            Console.WriteLine($"Creating '{name}'");

            var samlUpParty = new SamlUpParty
            {
                Name = name,
                SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                CertificateValidationMode = SamlUpPartyCertificateValidationMode.None,
                RevocationMode = SamlUpPartyRevocationMode.NoCheck,
                Issuer = "urn:itfoxtec:idservice:samples:aspnetcoresamlidpsample",
                AuthnBinding = new SamlBinding { RequestBinding = SamlBindingRequestBinding.Redirect, ResponseBinding = SamlBindingResponseBinding.Post },
                AuthnUrl = "https://localhost:44342/saml/login",
                Keys = new[] 
                { 
                    new JsonWebKey 
                    {
                        Kty = "RSA",
                        Kid = "27EB823D00B02FA7A02AA754146B6CFC60B8C301",
                        X5c = new[] { "MIICzzCCAbegAwIBAgIJANht5lyL71T0MA0GCSqGSIb3DQEBCwUAMBkxFzAVBgNVBAMTDnRlc3Qtc2lnbi1jZXJ0MB4XDTE4MTAxMTE1MDEyMVoXDTE5MTAxMjE1MDEyMVowGTEXMBUGA1UEAxMOdGVzdC1zaWduLWNlcnQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDUf6b4mtamR7DvTdtz6fdtEe+aXHpzXqvTrjf3SbN5Hol+kAvrIGVXcnJJSfO6N9yC/s8fPE4crVJKwceGkeykzt/j0UHRafmX7e7zzCPO8nd8pVSwZtflogNVdYbrIPTIHLP/eOrPt4Im2PHU0Q561frZjIDgqaoGmtpTLof/0z3GoD52hesZyeE3mW9Q0/+TngLne52rmDe9gmebtmckM7wJw9DXbaJhI24KZPn25PRYnPJMuyBh2EFjJ6qjIAQodpaMstdH6eGJyTan9J/yI6yPhYZ3jl4UngwZ7OpSiGB7m335SYIpPRGxZSdN/tGGdVPV1TIyBU6QFD5mn259AgMBAAGjGjAYMAkGA1UdEwQCMAAwCwYDVR0PBAQDAgXgMA0GCSqGSIb3DQEBCwUAA4IBAQCCYJoiq4sVqTyJ5md4VJIvT3Ezoo6MUDxPmC+bcdT+4j0rYPJr69Fv7celEcS7NEnDK3JQXU2bJ1HAAExBz53bqZphlFnuDFQcJU2lYGaOamDUN/v2aEM/g/Zrlbs4/V4WsCETUkcq7FwmtVia58AZSOtBEqpS7OpdEq4WUmWPRqpjDn+Ne1n921qIKMDtczqOCGc/BREbFUjy49gY/+a57WUxXPhL0gWrGHBwSLIJHp/m9sG7wFA6a2wnvgycrAMYFZ50iGe6IcSzktRdQXd5lTeVtl4JgftwIplIqWyuTYoHwTX+xo2qMSMCF38w31j6BASAmXJniKWeK8aeQ9o7" },
                        X5t = "J-uCPQCwL6egKqdUFGts_GC4wwE",
                        N = "1H-m-JrWpkew703bc-n3bRHvmlx6c16r064390mzeR6JfpAL6yBlV3JySUnzujfcgv7PHzxOHK1SSsHHhpHspM7f49FB0Wn5l-3u88wjzvJ3fKVUsGbX5aIDVXWG6yD0yByz_3jqz7eCJtjx1NEOetX62YyA4KmqBpraUy6H_9M9xqA-doXrGcnhN5lvUNP_k54C53udq5g3vYJnm7ZnJDO8CcPQ122iYSNuCmT59uT0WJzyTLsgYdhBYyeqoyAEKHaWjLLXR-nhick2p_Sf8iOsj4WGd45eFJ4MGezqUohge5t9-UmCKT0RsWUnTf7RhnVT1dUyMgVOkBQ-Zp9ufQ",
                        E = "AQAB"
                    }
                },
                LogoutBinding = new SamlBinding { RequestBinding = SamlBindingRequestBinding.Post, ResponseBinding = SamlBindingResponseBinding.Post },
                LogoutUrl = "https://localhost:44342/saml/logout"
            };

            await foxIDsApiClient.PostSamlUpPartyAsync(samlUpParty);

            Console.WriteLine($"'{name}' created");
        }

        private async Task CreateAspNetCoreApi1SampleDownPartyAsync()
        {
            var name = aspNetCoreApi1DownPartyName;
            Console.WriteLine($"Creating '{name}'");

            var oauthDownParty = new OAuthDownParty
            {
                Name = name,
                Resource = new OAuthDownResource
                {
                    Scopes = new[] { "admin", "some_access" }
                }
            };

            await foxIDsApiClient.PostOAuthDownPartyAsync(oauthDownParty);

            Console.WriteLine($"'{name}' created");
        }

        private async Task CreateAspNetCoreOidcAuthCodeSampleDownPartyAsync()
        {
            var name = aspNetCoreOidcAuthCodeDownPartyName;
            Console.WriteLine($"Creating '{name}'");

            var oidcDownParty = new OidcDownParty
            {
                Name = name,
                AllowCorsOrigins = new[] { "https://localhost:44340" },
                AllowUpPartyNames = new[] { "login", "aspnetcore_saml_idp_sample"/*, "adfs_saml_idp"*/ },
                Client = new OidcDownClient
                {
                    ResourceScopes = new[]
                    {
                        // Scope to the application it self.
                        new OAuthDownResourceScope { Resource = "aspnetcore_oidcauthcode_sample" },
                        // Scope to API1.
                        new OAuthDownResourceScope { Resource = "aspnetcore_api1_sample", Scopes = new [] { "admin", "some_access" } },
                    },
                    Scopes = new[]
                    {
                        new OidcDownScope { Scope = "offline_access" },
                        new OidcDownScope { Scope = "profile", VoluntaryClaims = new[]
                            {
                                new OidcDownClaim { Claim = "name", InIdToken = true  },
                                new OidcDownClaim { Claim = "family_name", InIdToken = true  },
                                new OidcDownClaim { Claim = "given_name", InIdToken = true  },
                                new OidcDownClaim { Claim = "middle_name", InIdToken = true  },
                                new OidcDownClaim { Claim = "nickname" },
                                new OidcDownClaim { Claim = "preferred_username" },
                                new OidcDownClaim { Claim = "profile" },
                                new OidcDownClaim { Claim = "picture" },
                                new OidcDownClaim { Claim = "website" },
                                new OidcDownClaim { Claim = "gender" },
                                new OidcDownClaim { Claim = "birthdate" },
                                new OidcDownClaim { Claim = "zoneinfo" },
                                new OidcDownClaim { Claim = "locale" },
                                new OidcDownClaim { Claim = "updated_at" }
                            }
                        },
                        new OidcDownScope { Scope = "email", VoluntaryClaims = new[]
                            {
                                new OidcDownClaim { Claim = "email", InIdToken = true  },
                                new OidcDownClaim { Claim = "email_verified" }
                            }
                        },
                        new OidcDownScope { Scope = "address", VoluntaryClaims = new[]
                            {
                                new OidcDownClaim { Claim = "address", InIdToken = true  }
                            }
                        },
                        new OidcDownScope { Scope = "phone", VoluntaryClaims = new[]
                            {
                                new OidcDownClaim { Claim = "phone_number", InIdToken = true  },
                                new OidcDownClaim { Claim = "phone_number_verified" },
                            }
                        },
                    },
                    Claims = new[]
                    {
                        new OidcDownClaim{ Claim = "email", InIdToken = true },
                        new OidcDownClaim{ Claim = "name", InIdToken = true },
                        new OidcDownClaim{ Claim = "family_name", InIdToken = true },
                        new OidcDownClaim{ Claim = "given_name", InIdToken = true },
                    },
                    ResponseTypes = new[] { "code", "code id_token token" },
                    RedirectUris = new[] { "https://localhost:44340/signin-oidc", "https://localhost:44340/signout-callback-oidc" },
                    RequireLogoutIdTokenHint = true,
                    AuthorizationCodeLifetime = 30,
                    IdTokenLifetime = 600,
                    AccessTokenLifetime = 600,
                    RefreshTokenLifetime = 900,
                    RefreshTokenAbsoluteLifetime = 1200,
                    RefreshTokenUseOneTime = false,
                    RefreshTokenLifetimeUnlimited = false
                }
            };

            await foxIDsApiClient.PostOidcDownPartyAsync(oidcDownParty); 

            Console.WriteLine($"'{name}' created");
        }
    }
}
