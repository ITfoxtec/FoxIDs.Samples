using FoxIDs.SampleSeedTool.Models;
using FoxIDs.SampleSeedTool.ServiceAccess;
using FoxIDs.SampleSeedTool.ServiceAccess.Contracts;
using ITfoxtec.Identity.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using UrlCombineLib;

namespace FoxIDs.SampleSeedTool.SeedLogic
{
    public class SampleSeedLogic
    {
        const string loginName = "login";

        const string aspNetCoreSamlIdPSampleUpPartyName = "aspnetcore_saml_idp_sample";

        const string aspNetCoreApi1SampleDownPartyName = "aspnetcore_api1_sample";

        const string aspNetCoreOidcAuthCodeSampleDownPartyName = "aspnetcore_oidcauthcode_sample";
        const string aspNetCoreOidcImplicitSampleDownPartyName = "aspnetcore_oidcimplicit_sample";
        const string blazorOidcAuthCodePkceSampleDownPartyName = "blazor_oidcauthcodepkce_sample";

        const string aspNetCoreSamlSampleDownPartyName = "aspnetcore_saml_sample";

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
            await CreateAspNetCoreOidcImplicitSampleDownPartyAsync();
            await CreateBlazorOidcAuthCodePkceSampleDownPartyAsync();

            await CreateAspNetCoreSamlSampleDownPartyAsync();

            Console.WriteLine(string.Empty);
            Console.WriteLine($"Sample configuration created");
        }

        public async Task DeleteAsync()
        {
            Console.WriteLine("Delete sample configuration");

            Console.WriteLine("Delete Oidc down party sample configuration");
            var oidcDownPartyNames = new[] { aspNetCoreOidcAuthCodeSampleDownPartyName, aspNetCoreOidcImplicitSampleDownPartyName, blazorOidcAuthCodePkceSampleDownPartyName };
            foreach (var name in oidcDownPartyNames)
            {
                try
                {
                    await foxIDsApiClient.DeleteOidcDownPartyAsync(name);
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

            Console.WriteLine("Delete OAuth down party sample configuration");
            var oauthDownPartyNames = new[] { aspNetCoreApi1SampleDownPartyName };
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

            Console.WriteLine("Delete SAML down party sample configuration");
            var samlDownPartyNames = new[] { aspNetCoreSamlSampleDownPartyName };
            foreach (var name in samlDownPartyNames)
            {
                try
                {
                    await foxIDsApiClient.DeleteSamlDownPartyAsync(name);
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
            var baseUrl = "https://localhost:44342";

            var samlUpParty = new SamlUpParty
            {
                Name = name,
                Issuer = "urn:itfoxtec:idservice:samples:aspnetcoresamlidpsample",
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
                //SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                //CertificateValidationMode = X509CertificateValidationMode.None,
                //RevocationMode = X509RevocationMode.NoCheck,
                AuthnBinding = new SamlBinding { RequestBinding = SamlBindingTypes.Redirect, ResponseBinding = SamlBindingTypes.Post },
                AuthnUrl = UrlCombine.Combine(baseUrl, "saml/login"),
                LogoutBinding = new SamlBinding { RequestBinding = SamlBindingTypes.Post, ResponseBinding = SamlBindingTypes.Post },
                LogoutUrl = UrlCombine.Combine(baseUrl, "saml/logout")
            };

            await foxIDsApiClient.PostSamlUpPartyAsync(samlUpParty);

            Console.WriteLine($"'{name}' created");
        }

        private async Task CreateAspNetCoreApi1SampleDownPartyAsync()
        {
            var name = aspNetCoreApi1SampleDownPartyName;
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
            var name = aspNetCoreOidcAuthCodeSampleDownPartyName;
            Console.WriteLine($"Creating '{name}'");
            var baseUrl = "https://localhost:44340";

            var oidcDownParty = new OidcDownParty
            {
                Name = name,
                AllowCorsOrigins = new[] { baseUrl },
                AllowUpPartyNames = new[] { "login", "aspnetcore_saml_idp_sample"/*, "adfs_saml_idp"*/ },
                Client = new OidcDownClient
                {
                    ResourceScopes = new[]
                    {
                        // Scope to the application it self.
                        new OAuthDownResourceScope { Resource = name },
                        // Scope to API1.
                        new OAuthDownResourceScope { Resource = "aspnetcore_api1_sample", Scopes = new [] { "admin", "some_access" } }
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
                    RedirectUris = new[] { UrlCombine.Combine(baseUrl, "signin-oidc"), UrlCombine.Combine(baseUrl, "signout-callback-oidc") },
                    RequireLogoutIdTokenHint = true,
                    AuthorizationCodeLifetime = 30, // 30 seconds 
                    IdTokenLifetime = 600, // 10 minutes
                    AccessTokenLifetime = 600, // 10 minutes
                    RefreshTokenLifetime = 900, // 15 minutes
                    RefreshTokenAbsoluteLifetime = 1200, // 20 minutes
                    RefreshTokenUseOneTime = false,
                    RefreshTokenLifetimeUnlimited = false
                }
            };

            await foxIDsApiClient.PostOidcDownPartyAsync(oidcDownParty);

            var secret = "KnhiOHuUz1zolY5k4B_r2M3iGkpkJmsmPwQ0RwS5KjM";
            await foxIDsApiClient.PostOidcClientSecretDownPartyAsync(new OAuthClientSecretRequest
            {
                PartyName = oidcDownParty.Name,
                Secrets = new string[] { secret },
            });
            Console.WriteLine($"'{name}' client secret is: {secret}");
            Console.WriteLine($"'{name}' created");
        }

        private async Task CreateAspNetCoreOidcImplicitSampleDownPartyAsync()
        {
            var name = aspNetCoreOidcImplicitSampleDownPartyName;
            Console.WriteLine($"Creating '{name}'");
            var baseUrl = "https://localhost:44341";

            var oidcDownParty = new OidcDownParty
            {
                Name = name,
                AllowCorsOrigins = new[] { baseUrl },
                AllowUpPartyNames = new[] { "login", "aspnetcore_saml_idp_sample"/*, "adfs_saml_idp"*/ },
                Client = new OidcDownClient
                {
                    ResourceScopes = new[]
                    {
                        // Scope to the application it self.
                        new OAuthDownResourceScope { Resource = name }
                    },
                    Scopes = new[]
                    {
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
                    },
                    ResponseTypes = new[] { "id_token token", "id_token" },
                    RedirectUris = new[] { UrlCombine.Combine(baseUrl, "signin-oidc"), UrlCombine.Combine(baseUrl, "signout-callback-oidc") },
                    RequireLogoutIdTokenHint = true,
                    IdTokenLifetime = 3600, // 60 minutes 
                    AccessTokenLifetime = 3600 // 60 minutes 
                }
            };

            await foxIDsApiClient.PostOidcDownPartyAsync(oidcDownParty);

            Console.WriteLine($"'{name}' created");
        }

        private async Task CreateBlazorOidcAuthCodePkceSampleDownPartyAsync()
        {
            var name = blazorOidcAuthCodePkceSampleDownPartyName;
            Console.WriteLine($"Creating '{name}'");
            var baseUrl = "https://localhost:44345";

            var oidcDownParty = new OidcDownParty
            {
                Name = name,
                AllowCorsOrigins = new[] { baseUrl },
                AllowUpPartyNames = new[] { "login", "aspnetcore_saml_idp_sample" },
                Client = new OidcDownClient
                {
                    ResourceScopes = new[]
                    {
                        // Scope to the application it self.
                        new OAuthDownResourceScope { Resource = name },
                        // Scope to API1.
                        new OAuthDownResourceScope { Resource = "aspnetcore_api1_sample", Scopes = new [] { "admin", "some_access" } }
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
                        new OidcDownClaim{ Claim = "role", InIdToken = true },
                    },
                    ResponseTypes = new[] { "code" },
                    RedirectUris = new[] { UrlCombine.Combine(baseUrl, "authentication/login-callback"), UrlCombine.Combine(baseUrl, "authentication/logout-callback") },
                    EnablePkce = true,
                    RequireLogoutIdTokenHint = true,
                    AuthorizationCodeLifetime = 30, // 30 seconds 
                    IdTokenLifetime = 600, // 10 minutes
                    AccessTokenLifetime = 600, // 10 minutes
                    RefreshTokenLifetime = 900, // 15 minutes
                    RefreshTokenAbsoluteLifetime = 1200, // 20 minutes
                    RefreshTokenUseOneTime = false,
                    RefreshTokenLifetimeUnlimited = false
                }
            };

            await foxIDsApiClient.PostOidcDownPartyAsync(oidcDownParty);

            var secret = RandomGenerator.Generate(32);
            await foxIDsApiClient.PostOidcClientSecretDownPartyAsync(new OAuthClientSecretRequest
            {
                PartyName = oidcDownParty.Name,
                Secrets = new string[] { secret },
            });
            Console.WriteLine($"'{name}' client secret is: {secret}");
            Console.WriteLine($"'{name}' created");
        }

        private async Task CreateAspNetCoreSamlSampleDownPartyAsync()
        {
            var name = aspNetCoreSamlSampleDownPartyName;
            Console.WriteLine($"Creating '{name}'");
            var baseUrl = "https://localhost:44343";

            var samlUpParty = new SamlDownParty
            {
                Name = name,
                Issuer = "urn:itfoxtec:idservice:samples:aspnetcoresamlsample",
                AllowUpPartyNames = new[] { "login", "aspnetcore_saml_idp_sample"/*, "adfs_saml_idp"*/ },
                Keys = new[]
                {
                    new JsonWebKey
                    {
                        Kty = "RSA",
                        Kid = "3863A8A752E5D6B812AA8A78A656E2DE6C637D12",
                        X5c = new[] { "MIICzzCCAbegAwIBAgIJAOd44ujQLBp/MA0GCSqGSIb3DQEBCwUAMBkxFzAVBgNVBAMTDnRlc3Qtc2lnbi1jZXJ0MB4XDTE4MTAwOTA4NTMxOFoXDTE5MTAxMDA4NTMxOFowGTEXMBUGA1UEAxMOdGVzdC1zaWduLWNlcnQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDCbttrAY2VkISBs/dQW9B38dvO1++Pcqqlj0darBfq8+1f+nRsn0OcQOYAhMvPhuS7qy5NLaTFm8RbH3veybYm7cJFU6xGu8SiLv6rPa5CBrSTgL/sJ+NwIDG3ZaZbayKTqgf31D1Gv8mIOWtEVHOn9ZPvfO6r0I9tLMZtJASHDTxe7niskT2PEfGe1KBTXVgJqY67KttzlydvH4zN+lwXFguBKLQqicw9iJ9BngxDAMLkOz6SIeF5WFGRPfiLD/MOZQ/skb+1H9Bl+5mbL/F0TiVs1HaQNEt3N9SO18dRyA2ZGtGfTzJbx3gQ7RwRjmNMnK8In9M0jxZZ1Rvji2XFAgMBAAGjGjAYMAkGA1UdEwQCMAAwCwYDVR0PBAQDAgXgMA0GCSqGSIb3DQEBCwUAA4IBAQC5RtTJV7mONXWKFkmF8EnCfPbFemCZs7Usw4cicjWlPTPfneFTsSJ4NuFmpWYrf1Lr75cf9BjHZDVHDGrRTsou/wAuqSehRPlZyj35ysjrC1hNmFYKQU+WkulxE4BZIcD+3fKj+6WAPVGG0NMnKWrmie2XK0aM5nFrWST4xqk6V5+4DOT7lltmPs9eUDJ8wkIL1oP/mhsE7tKpqMk9qNCb5nZMwXhqoTnlqTw/DFDCPJV/CS20/PamGTVUUhW1I0r73QDv054ycFY0ijU3tUK2V4D3daFTBHVGlLsCUxSBJSWkTGieN+iyU5aNbCErBc0+cim79lXT6sZ8VPVJ+kdW" },
                        X5t = "OGOop1Ll1rgSqop4plbi3mxjfRI",
                        N = "wm7bawGNlZCEgbP3UFvQd_Hbztfvj3KqpY9HWqwX6vPtX_p0bJ9DnEDmAITLz4bku6suTS2kxZvEWx973sm2Ju3CRVOsRrvEoi7-qz2uQga0k4C_7CfjcCAxt2WmW2sik6oH99Q9Rr_JiDlrRFRzp_WT73zuq9CPbSzGbSQEhw08Xu54rJE9jxHxntSgU11YCamOuyrbc5cnbx-MzfpcFxYLgSi0KonMPYifQZ4MQwDC5Ds-kiHheVhRkT34iw_zDmUP7JG_tR_QZfuZmy_xdE4lbNR2kDRLdzfUjtfHUcgNmRrRn08yW8d4EO0cEY5jTJyvCJ_TNI8WWdUb44tlxQ",
                        E = "AQAB"
                    }
                },
                SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                CertificateValidationMode = X509CertificateValidationMode.None,
                RevocationMode = X509RevocationMode.NoCheck,
                AuthnBinding = new SamlBinding { RequestBinding = SamlBindingTypes.Redirect, ResponseBinding = SamlBindingTypes.Post },
                AcsUrls = new[] { UrlCombine.Combine(baseUrl, "saml/assertionconsumerservice") },
                LogoutBinding = new SamlBinding { RequestBinding = SamlBindingTypes.Post, ResponseBinding = SamlBindingTypes.Post },
                SingleLogoutUrl = UrlCombine.Combine(baseUrl, "saml/singlelogout"),
                LoggedOutUrl = UrlCombine.Combine(baseUrl, "saml/loggedout"),
                MetadataLifetime = 1728000, // 20 days
                SubjectConfirmationLifetime = 300, // 5 minutes
                IssuedTokenLifetime = 36000 // 10 hours
            };

            await foxIDsApiClient.PostSamlDownPartyAsync(samlUpParty);

            Console.WriteLine($"'{name}' created");
        }


    }
}
