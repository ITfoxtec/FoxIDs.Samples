using FoxIDs.SampleSeedTool.Models;
using FoxIDs.SampleSeedTool.ServiceAccess;
using FoxIDs.SampleSeedTool.ServiceAccess.Contracts;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FoxIDs.SampleSeedTool.Logic
{
    public class SampleSeedLogic
    {
        const string loginName = "login";

        const string aspNetCoreSamlIdPSampleUpPartyName = "aspnetcore_saml_idp_sample";
        const string identityserverOidcOpUpPartyName = "identityserver_oidc_op_sample";

        const string aspNetCoreApi1SampleDownPartyName = "aspnetcore_api1_sample";
        const string aspNetCoreApi2SampleDownPartyName = "aspnetcore_api2_sample";
        const string aspNetCoreApiOAuthTwoIdPsSampleDownPartyName = "aspnetapi_oauth_twoidps_sample";

        const string netCoreClientGrantConsoleSampleDownPartyName = "netcore_clientgrant_sample";
        const string netCoreClientAssertionGrantConsoleSampleDownPartyName = "netcore_clientassgrant_sample";

        const string aspNetCoreOidcAuthCoreAllUpPartiesSampleDownPartyName = "aspnet_oidc_allup_sample";
        // Optional, to enable token exchange trust through up-party instead of the default in same track trust.
        const string aspNetCoreOidcAuthCoreAllUpPartiesSampleOAuthTrustUpPartyName = "aspnet_oidc_allup_sample-trust";
        const string aspNetCoreOidcAuthCodeSampleDownPartyName = "aspnetcore_oidcauthcode_sample";
        const string aspNetCoreOidcImplicitSampleDownPartyName = "aspnetcore_oidcimplicit_sample"; 
        const string blazorBffAspNetCoreOidcSampleDownPartyName = "blazor_bff_aspnet_oidc_sample";
        const string blazorOidcAuthCodePkceSampleDownPartyName = "blazor_oidcauthcodepkce_sample";
        const string blazorServerOidcSampleDownPartyName = "blazorserver_oidc_sample";

        const string aspNetCoreSamlSampleDownPartyName = "aspnetcore_saml_sample";
        const string aspNetCoreSamlSampleDownIssuer = "urn:itfoxtec:idservice:samples:aspnetcoresamlsample";
        const string oauthTokenExchangeForSamlDownPartyName = "token_exchange_saml";
        // To enable token exchange trust
        const string aspNetCoreSamlSampleTrustUpPartyName = "aspnetcore_saml_sample-trust";

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

            await CreateAspNetCoreSamlIdPSampleUpPartyAsync();
            await CreateIdentityserverOidcOpUpPartyAsync();

            await CreateAspNetCoreApi2SampleDownPartyAsync();
            await CreateAspNetCoreApi1SampleDownPartyAsync();
            await CreateAspNetCoreApiOAuthTwoIdPsSampleDownPartyAsync();

            await CreateNetCoreClientGrantConsoleSampleDownPartyAsync();
            await CreateNetCoreClientAssertionGrantConsoleSampleDownPartyAsync();

            await CreateAspNetCoreOidcAuthCoreAllUpPartiesSampleOAuthTrustUpPartyAsync();
            await CreateAspNetCoreOidcAuthCoreAllUpPartiesSampleDownPartyAsync();
            await CreateAspNetCoreOidcAuthCodeSampleDownPartyAsync();
            await CreateAspNetCoreOidcImplicitSampleDownPartyAsync();
            await CreateBffAspNetCoreOidcSampleDownPartyAsync();
            await CreateBlazorOidcAuthCodePkceSampleDownPartyAsync();
            await CreateblazorServerOidcSampleDownPartyAsync();

            await CreateAspNetCoreSamlSampleDownPartyAsync();
            await CreateAspNetCoreSamlSampleTrustUpPartyAsync();
            await CreateOAuthTokenExchangeForSamlDownPartyAsync();

            Console.WriteLine(string.Empty);
            Console.WriteLine($"Sample configuration created");
        }

        public async Task DeleteAsync()
        {
            Console.WriteLine("Delete sample configuration");

            Console.WriteLine("Delete Oidc down party sample configuration");
            var oidcDownPartyNames = new[] { aspNetCoreOidcAuthCoreAllUpPartiesSampleDownPartyName, aspNetCoreOidcAuthCodeSampleDownPartyName, aspNetCoreOidcImplicitSampleDownPartyName, blazorBffAspNetCoreOidcSampleDownPartyName, blazorOidcAuthCodePkceSampleDownPartyName, blazorServerOidcSampleDownPartyName, oauthTokenExchangeForSamlDownPartyName };
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
            var oauthDownPartyNames = new[] { netCoreClientGrantConsoleSampleDownPartyName, netCoreClientAssertionGrantConsoleSampleDownPartyName, aspNetCoreApi1SampleDownPartyName, aspNetCoreApi2SampleDownPartyName, aspNetCoreApiOAuthTwoIdPsSampleDownPartyName };
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
            var samlUpPartyNames = new[] { aspNetCoreSamlIdPSampleUpPartyName, aspNetCoreSamlSampleTrustUpPartyName };
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

            Console.WriteLine("Delete OAuth up party sample configuration");
            var oauthUpPartyNames = new[] { aspNetCoreOidcAuthCoreAllUpPartiesSampleOAuthTrustUpPartyName };
            foreach (var name in oauthUpPartyNames)
            {
                try
                {
                    await foxIDsApiClient.DeleteOAuthUpPartyAsync(name);
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

            Console.WriteLine("Delete OIDC up party sample configuration");
            var oidcUpPartyNames = new[] { identityserverOidcOpUpPartyName };
            foreach (var name in oidcUpPartyNames)
            {
                try
                {
                    await foxIDsApiClient.DeleteOidcUpPartyAsync(name);
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

        private async Task CreateAspNetCoreSamlIdPSampleUpPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetSamlUpPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44342";

                var samlUpParty = new SamlUpParty
                {
                    Name = name,
                    Issuer = "urn:itfoxtec:idservice:samples:aspnetcoresamlidpsample",
                    Keys = new[] { GetCertificateInfoKey("AspNetCoreSamlIdPSample-test-sign-cert.crt") },
                    //SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                    //CertificateValidationMode = X509CertificateValidationMode.None,
                    //RevocationMode = X509RevocationMode.NoCheck,
                    AuthnRequestBinding = SamlBindingTypes.Redirect,
                    AuthnResponseBinding = SamlBindingTypes.Post,
                    AuthnUrl = UrlCombine.Combine(baseUrl, "saml/login"),
                    LogoutRequestBinding = SamlBindingTypes.Post,
                    LogoutResponseBinding = SamlBindingTypes.Post,
                    LogoutUrl = UrlCombine.Combine(baseUrl, "saml/logout"),
                    Claims = new string[] { ClaimTypes.Email, ClaimTypes.Name, ClaimTypes.GivenName, ClaimTypes.Surname, ClaimTypes.Role },
                    SessionLifetime = 36000,
                    SessionAbsoluteLifetime = 86400,
                };

                await foxIDsApiClient.PostSamlUpPartyAsync(samlUpParty);
            };

            await CreateIfNotExistsAsync(aspNetCoreSamlIdPSampleUpPartyName, getAction, postAction);
        }

        private async Task CreateIdentityserverOidcOpUpPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOidcUpPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44346";

                var key = File.ReadAllText("identityserver-tempkey.jwk").ToObject<JwkWithCertificateInfo>();

                var oidcUpParty = new OidcUpParty
                {
                    Name = name,
                    Authority = baseUrl,
                    UpdateState = PartyUpdateStates.Manual,
                    Issuers = new string[] { baseUrl },
                    Keys = new JwkWithCertificateInfo[] { key },

                    Client = new OidcUpClient
                    {
                        AuthorizeUrl = $"{baseUrl}/connect/authorize",
                        TokenUrl = $"{baseUrl}/connect/token",
                        EndSessionUrl = $"{baseUrl}/connect/endsession",

                        // A less secure configuration to enable local testing
                        ResponseType = IdentityConstants.ResponseTypes.IdToken,

                        // A more secure configuration
                        //ResponseType = IdentityConstants.ResponseTypes.Code,
                        //EnablePkce = true,
                        //ClientSecret = "2tqjW-KwiGaR4KRt0IJ8KAJYw3pyPTK8S_dr_YE5nbw",

                        Scopes = new string[] { "profile", "email" },
                        Claims = new string[] { "access_token", JwtClaimTypes.Email, JwtClaimTypes.EmailVerified, JwtClaimTypes.FamilyName, JwtClaimTypes.GivenName, JwtClaimTypes.Name, JwtClaimTypes.Role },

                        UseIdTokenClaims = true,

                        ResponseMode = IdentityConstants.ResponseModes.FormPost
                    },
                    SessionLifetime = 36000,
                    SessionAbsoluteLifetime = 86400,
                };

                await foxIDsApiClient.PostOidcUpPartyAsync(oidcUpParty);
            };

            await CreateIfNotExistsAsync(identityserverOidcOpUpPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreApi2SampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOAuthDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var oauthDownParty = new OAuthDownParty
                {
                    Name = name,
                    Resource = new OAuthDownResource
                    {
                        Scopes = new[] { "admin", "some_2_access" }
                    }
                };

                await foxIDsApiClient.PostOAuthDownPartyAsync(oauthDownParty);
            };

            await CreateIfNotExistsAsync(aspNetCoreApi2SampleDownPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreApi1SampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOAuthDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var oauthDownParty = new OAuthDownParty
                {                    
                    Name = name,                    
                    // The client is configured to support token exchange from tokens issued to the API in the same track.
                    Client = new OAuthDownClient
                    {
                        DisableClientCredentialsGrant = true,
                        ResourceScopes = new [] 
                        {
                            // Scope to API2.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi2SampleDownPartyName, Scopes = new[] { "some_2_access" } }
                        },
                        AccessTokenLifetime = 600, // 10 minutes
                        ClientAuthenticationMethod = ClientAuthenticationMethods.PrivateKeyJwt,
                        ClientKeys = new[] { GetCertificateInfoKey("CN=AspNetCoreApi1Sample, O=test corp.cer") }
                    },
                    Resource = new OAuthDownResource
                    {
                        Scopes = new[] { "admin", "some_access" }
                    }
                };

                await foxIDsApiClient.PostOAuthDownPartyAsync(oauthDownParty);
            };

            await CreateIfNotExistsAsync(aspNetCoreApi1SampleDownPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreApiOAuthTwoIdPsSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOAuthDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var oauthDownParty = new OAuthDownParty
                {
                    Name = name,
                    Resource = new OAuthDownResource
                    {
                        Scopes = new[] { "admin", "some_access" }
                    }
                };

                await foxIDsApiClient.PostOAuthDownPartyAsync(oauthDownParty);
            };

            await CreateIfNotExistsAsync(aspNetCoreApiOAuthTwoIdPsSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateNetCoreClientGrantConsoleSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOAuthDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var oauthDownParty = new OAuthDownParty
                {
                    Name = name,
                    Client = new OAuthDownClient
                    {
                        ResourceScopes = new[]
                        {
                            // Scope to API1.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi1SampleDownPartyName, Scopes = new [] { "admin", "some_access" } },
                            // Scope to API which support two IdPs.
                            new OAuthDownResourceScope { Resource = aspNetCoreApiOAuthTwoIdPsSampleDownPartyName, Scopes = new [] { "some_access" } }
                        },
                        AccessTokenLifetime = 600 // 10 minutes
                    }
                };

                await foxIDsApiClient.PostOAuthDownPartyAsync(oauthDownParty);

                var secret = "MXtV-UmVJqygGUthkG5Q_6SCpmyBpsksvA1kvbE735k";
                await foxIDsApiClient.PostOAuthClientSecretDownPartyAsync(new OAuthClientSecretRequest
                {
                    PartyName = oauthDownParty.Name,
                    Secrets = new string[] { secret },
                });
                Console.WriteLine($"\t'{name}' client secret is: {secret}");
            };

            await CreateIfNotExistsAsync(netCoreClientGrantConsoleSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateNetCoreClientAssertionGrantConsoleSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOAuthDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var oauthDownParty = new OAuthDownParty
                {
                    Name = name,
                    Client = new OAuthDownClient
                    {
                        ResourceScopes = new[]
                        {
                            // Scope to API1.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi1SampleDownPartyName, Scopes = new [] { "admin", "some_access" } },
                            // Scope to API which support two IdPs.
                            new OAuthDownResourceScope { Resource = aspNetCoreApiOAuthTwoIdPsSampleDownPartyName, Scopes = new [] { "some_access" } }
                        },
                        AccessTokenLifetime = 600, // 10 minutes
                        ClientAuthenticationMethod = ClientAuthenticationMethods.PrivateKeyJwt,
                        ClientKeys = new [] { GetCertificateInfoKey("CN=NetCoreClientAssertionGrantConsoleSample, O=test corp.cer") }
                    }
                };

                await foxIDsApiClient.PostOAuthDownPartyAsync(oauthDownParty);
            };

            await CreateIfNotExistsAsync(netCoreClientAssertionGrantConsoleSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreOidcAuthCoreAllUpPartiesSampleOAuthTrustUpPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOAuthUpPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var oauthUpParty = new OAuthUpParty
                {
                    DisableUserAuthenticationTrust = true,
                    Name = name,
                    Note = $"Trust {aspNetCoreOidcAuthCoreAllUpPartiesSampleDownPartyName} to enable token exchange trust through up-party instead of the default in same track trust.",
                    UpdateState = PartyUpdateStates.Automatic,
                    Authority = UrlCombine.Combine(settings.FoxIDsEndpoint, new[] { settings.Tenant, settings.Track, aspNetCoreOidcAuthCoreAllUpPartiesSampleDownPartyName }),
                    SpIssuer = aspNetCoreOidcAuthCoreAllUpPartiesSampleDownPartyName,
                    Client = new OAuthUpClient
                    {
                        Claims = new string[] { "*" }
                    }
                };

                await foxIDsApiClient.PostOAuthUpPartyAsync(oauthUpParty);
            };

            await CreateIfNotExistsAsync(aspNetCoreOidcAuthCoreAllUpPartiesSampleOAuthTrustUpPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreOidcAuthCoreAllUpPartiesSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOidcDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44349";

                var oidcDownParty = new OidcDownParty
                {
                    Name = name,
                    AllowCorsOrigins = new[] { baseUrl },
                    // Enable aspNetCoreOidcAuthCoreAllUpPartiesSampleOAuthTrustUpPartyName in AllowUpPartyNames to do token exchange through up-party trust.
                    AllowUpPartyNames = new[] { loginName, aspNetCoreSamlIdPSampleUpPartyName, identityserverOidcOpUpPartyName/*, aspNetCoreOidcAuthCoreAllUpPartiesSampleOAuthTrustUpPartyName*/ /*, "foxids_oidcpkce", "adfs_saml_idp" */ },
                    Client = new OidcDownClient
                    {
                        ResourceScopes = new[]
                        {
                            // Scope to the application it self, then the client can do token exchange.
                            new OAuthDownResourceScope { Resource = name },
                            // Scope to API1.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi1SampleDownPartyName, Scopes = new [] { "admin", "some_access" } },
                            // Scope to API2 - accessed with token exchange.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi2SampleDownPartyName, Scopes = new [] { "some_2_access" } }
                        },
                        Scopes = new[]
                        {
                            new OidcDownScope { Scope = "offline_access" },
                            new OidcDownScope { Scope = "profile", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Name, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.FamilyName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.GivenName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.MiddleName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Nickname },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PreferredUsername },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Profile },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Picture },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Website },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Gender },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Birthdate },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Zoneinfo },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Locale },
                                    new OidcDownClaim { Claim = JwtClaimTypes.UpdatedAt }
                                }
                            },
                            new OidcDownScope { Scope = "email", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Email, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.EmailVerified }
                                }
                            },
                            new OidcDownScope { Scope = "address", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Address, InIdToken = true  }
                                }
                            },
                            new OidcDownScope { Scope = "phone", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumber, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumberVerified },
                                }
                            },
                        },
                        Claims = new[]
                        {
                            new OidcDownClaim{ Claim = JwtClaimTypes.Email, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Name, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.FamilyName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.GivenName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Role, InIdToken = true }
                        },
                        ResponseTypes = new[] { "code" },
                        RedirectUris = new[] { UrlCombine.Combine(baseUrl, "signin-oidc") },
                        PostLogoutRedirectUri = UrlCombine.Combine(baseUrl, "signout-callback-oidc"),
                        FrontChannelLogoutUri = UrlCombine.Combine(baseUrl, "auth", "frontchannellogout"),
                        FrontChannelLogoutSessionRequired = true,
                        RequirePkce = true,
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

                var secret = "IxIruKswG4sQxzOrKlXR58strgZtoyZPG18J3FhzEXI";
                await foxIDsApiClient.PostOidcClientSecretDownPartyAsync(new OAuthClientSecretRequest
                {
                    PartyName = oidcDownParty.Name,
                    Secrets = new string[] { secret },
                });
                Console.WriteLine($"\t'{name}' client secret is: {secret}");
            };

            await CreateIfNotExistsAsync(aspNetCoreOidcAuthCoreAllUpPartiesSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreOidcAuthCodeSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOidcDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44340";

                var oidcDownParty = new OidcDownParty
                {
                    Name = name,
                    AllowCorsOrigins = new[] { baseUrl },
                    AllowUpPartyNames = new[] { loginName, aspNetCoreSamlIdPSampleUpPartyName, identityserverOidcOpUpPartyName/*, "foxids_oidcpkce", "adfs_saml_idp"*/ },
                    Client = new OidcDownClient
                    {
                        ResourceScopes = new[]
                        {
                            // Scope to the application it self.
                            //new OAuthDownResourceScope { Resource = name },
                            // Scope to API1.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi1SampleDownPartyName, Scopes = new [] { "admin", "some_access" } }
                        },
                        Scopes = new[]
                        {
                            new OidcDownScope { Scope = "offline_access" },
                            new OidcDownScope { Scope = "profile", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Name, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.FamilyName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.GivenName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.MiddleName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Nickname },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PreferredUsername },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Profile },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Picture },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Website },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Gender },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Birthdate },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Zoneinfo },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Locale },
                                    new OidcDownClaim { Claim = JwtClaimTypes.UpdatedAt }
                                }
                            },
                            new OidcDownScope { Scope = "email", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Email, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.EmailVerified }
                                }
                            },
                            new OidcDownScope { Scope = "address", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Address, InIdToken = true  }
                                }
                            },
                            new OidcDownScope { Scope = "phone", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumber, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumberVerified },
                                }
                            },
                        },
                        Claims = new[]
                        {
                            new OidcDownClaim{ Claim = JwtClaimTypes.Email, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Name, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.FamilyName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.GivenName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Role, InIdToken = true }
                        },
                        ResponseTypes = new[] { "code" },
                        RedirectUris = new[] { UrlCombine.Combine(baseUrl, "signin-oidc") },
                        PostLogoutRedirectUri = UrlCombine.Combine(baseUrl, "signout-callback-oidc"),
                        FrontChannelLogoutUri = UrlCombine.Combine(baseUrl, "auth", "frontchannellogout"),
                        FrontChannelLogoutSessionRequired = true,
                        RequirePkce = true,
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
                Console.WriteLine($"\t'{name}' client secret is: {secret}");
            };

            await CreateIfNotExistsAsync(aspNetCoreOidcAuthCodeSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreOidcImplicitSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOidcDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44341";

                var oidcDownParty = new OidcDownParty
                {
                    Name = name,
                    AllowCorsOrigins = new[] { baseUrl },
                    AllowUpPartyNames = new[] { loginName, aspNetCoreSamlIdPSampleUpPartyName, identityserverOidcOpUpPartyName/*, "foxids_oidcpkce", "adfs_saml_idp"*/ },
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
                                    new OidcDownClaim { Claim = JwtClaimTypes.Name, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.FamilyName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.GivenName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.MiddleName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Nickname },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PreferredUsername },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Profile },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Picture },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Website },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Gender },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Birthdate },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Zoneinfo },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Locale },
                                    new OidcDownClaim { Claim = JwtClaimTypes.UpdatedAt }
                                }
                            },
                            new OidcDownScope { Scope = "email", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Email, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.EmailVerified }
                                }
                            },
                        },
                        Claims = new[]
                        {
                            new OidcDownClaim{ Claim = JwtClaimTypes.Role, InIdToken = true }
                        },
                        ResponseTypes = new[] { "id_token token", "id_token" },
                        RedirectUris = new[] { UrlCombine.Combine(baseUrl, "signin-oidc") },
                        PostLogoutRedirectUri = UrlCombine.Combine(baseUrl, "signout-callback-oidc"),
                        FrontChannelLogoutUri = UrlCombine.Combine(baseUrl, "auth", "frontchannellogout"),
                        FrontChannelLogoutSessionRequired = true,

                        RequirePkce = false,
                        RequireLogoutIdTokenHint = true,
                        IdTokenLifetime = 3600, // 60 minutes 
                        AccessTokenLifetime = 3600 // 60 minutes 
                    }
                };

                await foxIDsApiClient.PostOidcDownPartyAsync(oidcDownParty);
            };

            await CreateIfNotExistsAsync(aspNetCoreOidcImplicitSampleDownPartyName, getAction, postAction);
        }

        

        private async Task CreateBffAspNetCoreOidcSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOidcDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44348";

                var oidcDownParty = new OidcDownParty
                {
                    Name = name,
                    AllowCorsOrigins = new[] { baseUrl },
                    AllowUpPartyNames = new[] { loginName, aspNetCoreSamlIdPSampleUpPartyName, identityserverOidcOpUpPartyName/*, "foxids_oidcpkce", "adfs_saml_idp"*/ },
                    Client = new OidcDownClient
                    {
                        ResourceScopes = new[]
                        {
                            // Scope to the application it self.
                            //new OAuthDownResourceScope { Resource = name },
                            // Scope to API1.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi1SampleDownPartyName, Scopes = new [] { "admin", "some_access" } }
                        },
                        Scopes = new[]
                        {
                            new OidcDownScope { Scope = "offline_access" },
                            new OidcDownScope { Scope = "profile", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Name, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.FamilyName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.GivenName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.MiddleName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Nickname },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PreferredUsername },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Profile },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Picture },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Website },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Gender },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Birthdate },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Zoneinfo },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Locale },
                                    new OidcDownClaim { Claim = JwtClaimTypes.UpdatedAt }
                                }
                            },
                            new OidcDownScope { Scope = "email", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Email, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.EmailVerified }
                                }
                            },
                            new OidcDownScope { Scope = "address", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Address, InIdToken = true  }
                                }
                            },
                            new OidcDownScope { Scope = "phone", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumber, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumberVerified },
                                }
                            },
                        },
                        Claims = new[]
                        {
                            new OidcDownClaim{ Claim = JwtClaimTypes.Email, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Name, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.FamilyName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.GivenName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Role, InIdToken = true }
                        },
                        ResponseTypes = new[] { "code" },
                        RedirectUris = new[] { UrlCombine.Combine(baseUrl, "signin-oidc") },
                        PostLogoutRedirectUri = UrlCombine.Combine(baseUrl, "signout-callback-oidc"),
                        FrontChannelLogoutUri = UrlCombine.Combine(baseUrl, "auth", "frontchannellogout"),
                        FrontChannelLogoutSessionRequired = true,
                        RequirePkce = true,
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

                var secret = "wXyFfKVxZXGAIoFrcj-8hXtcPD6CgtjpEhrqGJJe95g";
                await foxIDsApiClient.PostOidcClientSecretDownPartyAsync(new OAuthClientSecretRequest
                {
                    PartyName = oidcDownParty.Name,
                    Secrets = new string[] { secret },
                });
                Console.WriteLine($"\t'{name}' client secret is: {secret}");
            };

            await CreateIfNotExistsAsync(blazorBffAspNetCoreOidcSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateBlazorOidcAuthCodePkceSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOidcDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44345";

                var oidcDownParty = new OidcDownParty
                {
                    Name = name,
                    AllowCorsOrigins = new[] { baseUrl },
                    AllowUpPartyNames = new[] { loginName, aspNetCoreSamlIdPSampleUpPartyName, identityserverOidcOpUpPartyName/*, "foxids_oidcpkce", "adfs_saml_idp"*/ },
                    Client = new OidcDownClient
                    {
                        ResourceScopes = new[]
                        {
                            // Scope to the application it self.
                            //new OAuthDownResourceScope { Resource = name },
                            // Scope to API1.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi1SampleDownPartyName, Scopes = new [] { "admin", "some_access" } }
                        },
                        Scopes = new[]
                        {
                            new OidcDownScope { Scope = "offline_access" },
                            new OidcDownScope { Scope = "profile", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Name, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.FamilyName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.GivenName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.MiddleName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Nickname },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PreferredUsername },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Profile },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Picture },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Website },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Gender },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Birthdate },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Zoneinfo },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Locale },
                                    new OidcDownClaim { Claim = JwtClaimTypes.UpdatedAt }
                                }
                            },
                            new OidcDownScope { Scope = "email", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Email, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.EmailVerified }
                                }
                            },
                            new OidcDownScope { Scope = "address", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Address, InIdToken = true  }
                                }
                            },
                            new OidcDownScope { Scope = "phone", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumber, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumberVerified },
                                }
                            },
                        },
                        Claims = new[]
                        {
                            new OidcDownClaim{ Claim = JwtClaimTypes.Email, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Name, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.FamilyName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.GivenName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Role, InIdToken = true }
                        },
                        ResponseTypes = new[] { "code" },
                        RedirectUris = new[] { UrlCombine.Combine(baseUrl, "authentication/login-callback") },
                        PostLogoutRedirectUri = UrlCombine.Combine(baseUrl, "authentication/logout-callback"),
                        RequirePkce = true,
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
            };

            await CreateIfNotExistsAsync(blazorOidcAuthCodePkceSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateblazorServerOidcSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOidcDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44347";

                var oidcDownParty = new OidcDownParty
                {
                    Name = name,
                    AllowCorsOrigins = new[] { baseUrl },
                    AllowUpPartyNames = new[] { loginName, aspNetCoreSamlIdPSampleUpPartyName, identityserverOidcOpUpPartyName/*, "foxids_oidcpkce", "adfs_saml_idp"*/ },
                    Client = new OidcDownClient
                    {
                        ResourceScopes = new[]
                        {
                            // Scope to the application it self.
                            //new OAuthDownResourceScope { Resource = name },
                            // Scope to API1.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi1SampleDownPartyName, Scopes = new [] { "admin", "some_access" } }
                        },
                        Scopes = new[]
                        {
                            new OidcDownScope { Scope = "offline_access" },
                            new OidcDownScope { Scope = "profile", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Name, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.FamilyName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.GivenName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.MiddleName, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Nickname },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PreferredUsername },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Profile },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Picture },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Website },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Gender },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Birthdate },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Zoneinfo },
                                    new OidcDownClaim { Claim = JwtClaimTypes.Locale },
                                    new OidcDownClaim { Claim = JwtClaimTypes.UpdatedAt }
                                }
                            },
                            new OidcDownScope { Scope = "email", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Email, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.EmailVerified }
                                }
                            },
                            new OidcDownScope { Scope = "address", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.Address, InIdToken = true  }
                                }
                            },
                            new OidcDownScope { Scope = "phone", VoluntaryClaims = new[]
                                {
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumber, InIdToken = true  },
                                    new OidcDownClaim { Claim = JwtClaimTypes.PhoneNumberVerified },
                                }
                            },
                        },
                        Claims = new[]
                        {
                            new OidcDownClaim{ Claim = JwtClaimTypes.Email, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Name, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.FamilyName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.GivenName, InIdToken = true },
                            new OidcDownClaim{ Claim = JwtClaimTypes.Role, InIdToken = true }
                        },
                        ResponseTypes = new[] { "code" },
                        RedirectUris = new[] { UrlCombine.Combine(baseUrl, "signin-oidc") },
                        PostLogoutRedirectUri = UrlCombine.Combine(baseUrl, "signout-callback-oidc"),
                        FrontChannelLogoutUri = UrlCombine.Combine(baseUrl, "auth", "frontchannellogout"),
                        FrontChannelLogoutSessionRequired = true,
                        RequirePkce = true,
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

                var secret = "rbZ82PSaE0NeRLMy4DNhlP1mnsdcgMfjnF5niufa1w0";
                await foxIDsApiClient.PostOidcClientSecretDownPartyAsync(new OAuthClientSecretRequest
                {
                    PartyName = oidcDownParty.Name,
                    Secrets = new string[] { secret },
                });
                Console.WriteLine($"\t'{name}' client secret is: {secret}");
            };

            await CreateIfNotExistsAsync(blazorServerOidcSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreSamlSampleDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetSamlDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var baseUrl = "https://localhost:44343";

                var samlUpParty = new SamlDownParty
                {
                    Name = name,
                    Issuer = aspNetCoreSamlSampleDownIssuer,
                    AllowUpPartyNames = new[] { loginName, aspNetCoreSamlIdPSampleUpPartyName, identityserverOidcOpUpPartyName/*, "foxids_oidcpkce", "adfs_saml_idp"*/ },
                    Keys = new[] { GetCertificateInfoKey("AspNetCoreSamlSample-test-sign-cert.crt") },
                    SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck,
                    AuthnResponseSignType = Saml2AuthnResponseSignTypes.SignAssertionAndResponse,
                    AuthnRequestBinding = SamlBindingTypes.Redirect,
                    AuthnResponseBinding = SamlBindingTypes.Post,
                    AcsUrls = new[] { UrlCombine.Combine(baseUrl, "saml/assertionconsumerservice") },
                    LogoutRequestBinding = SamlBindingTypes.Post,
                    LogoutResponseBinding = SamlBindingTypes.Post,
                    SingleLogoutUrl = UrlCombine.Combine(baseUrl, "saml/singlelogout"),
                    LoggedOutUrl = UrlCombine.Combine(baseUrl, "saml/loggedout"),
                    Claims = new string[] { ClaimTypes.Email, ClaimTypes.Name, ClaimTypes.GivenName, ClaimTypes.Surname, ClaimTypes.Role },
                    SubjectConfirmationLifetime = 300, // 5 minutes
                    IssuedTokenLifetime = 36000 // 10 hours
                };

                await foxIDsApiClient.PostSamlDownPartyAsync(samlUpParty);
            };

            await CreateIfNotExistsAsync(aspNetCoreSamlSampleDownPartyName, getAction, postAction);
        }

        private async Task CreateAspNetCoreSamlSampleTrustUpPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetSamlUpPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var samlUpParty = new SamlUpParty
                {
                    // Only enable token exchange
                    DisableUserAuthenticationTrust = true,
                    Name = name,
                    UpdateState = PartyUpdateStates.Automatic,
                    MetadataUrl = UrlCombine.Combine(settings.FoxIDsEndpoint, new[] { settings.Tenant, settings.Track, aspNetCoreSamlSampleDownPartyName, "saml", "idpmetadata" }),
                    // The SP issuer can be configured to match an external application (service provider)
                    SpIssuer = aspNetCoreSamlSampleDownIssuer,
                };

                await foxIDsApiClient.PostSamlUpPartyAsync(samlUpParty);
            };

            await CreateIfNotExistsAsync(aspNetCoreSamlSampleTrustUpPartyName, getAction, postAction);
        }

        private async Task CreateOAuthTokenExchangeForSamlDownPartyAsync()
        {
            Func<string, Task> getAction = async (name) =>
            {
                _ = await foxIDsApiClient.GetOAuthDownPartyAsync(name);
            };

            Func<string, Task> postAction = async (name) =>
            {
                var oauthDownParty = new OAuthDownParty
                {
                    Name = name,
                    AllowUpPartyNames = new[] { aspNetCoreSamlSampleTrustUpPartyName },
                    Client = new OAuthDownClient
                    {
                        ResourceScopes = new[]
                        {
                            // Scope to API1.
                            new OAuthDownResourceScope { Resource = aspNetCoreApi1SampleDownPartyName, Scopes = new [] { "some_access" } },
                        },
                        AccessTokenLifetime = 600, // 10 minutes
                        ClientAuthenticationMethod = ClientAuthenticationMethods.PrivateKeyJwt,
                        ClientKeys = new[] { GetCertificateInfoKey("CN=TokenExchangeAspnetcoreSamlSample, O=test corp.cer") }
                    }
                };

                await foxIDsApiClient.PostOAuthDownPartyAsync(oauthDownParty);
            };

            await CreateIfNotExistsAsync(oauthTokenExchangeForSamlDownPartyName, getAction, postAction);
        }        

        private JwkWithCertificateInfo GetCertificateInfoKey(string file)
        {
            var certificate = CertificateUtil.Load(file);
            var jwk = certificate.ToFTJsonWebKey();
            return jwk.ToJson().ToObject<JwkWithCertificateInfo>();
        }

        private async Task CreateIfNotExistsAsync(string name, Func<string, Task> getActionAsync, Func<string, Task> postActionAsync)
        {
            Console.WriteLine($"Creating '{name}'");

            try
            {
                await getActionAsync(name);

                Console.WriteLine($"\t'{name}' already exists");
            }
            catch (FoxIDsApiException ex)
            {
                if (ex.StatusCode == StatusCodes.Status404NotFound)
                {
                    await postActionAsync(name);

                    Console.WriteLine($"\t'{name}' created");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
