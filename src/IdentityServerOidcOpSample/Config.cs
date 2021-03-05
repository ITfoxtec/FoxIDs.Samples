// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using IdentityServerOidcOpSample.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public class Config
    {
        private readonly IdentitySettings settings;

        public Config(IdentitySettings settings)
        {
            this.settings = settings;
        }

        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Profile();
            yield return new IdentityResources.Email();
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new ApiResource[] { };
            //yield return new ApiResource("some.api", "Some API")
            //{
            //    UserClaims = new[] { "email", "email_verified", "family_name", "given_name", "name", "role" },

            //    Scopes = new List<string>
            //    {
            //        "some.api.access"
            //    }
            //};
        }

        public IEnumerable<ApiScope> GetApiScopes()
        {
            return new ApiScope[] { };
            //yield return new ApiScope("some.api.access", "Some API scope") ;
        }

        public IEnumerable<Client> GetClients()
        {
            foreach (var clientSettings in settings.OidcClients)
            {
                yield return new Client
                {
                    ClientId = clientSettings.ClientId,

                    // A less secure configuration to enable testing
                    AllowedGrantTypes = GrantTypes.Implicit,

                    // A more secure configuration
                    //AllowedGrantTypes = GrantTypes.Code,
                    //RequirePkce = true,
                    //ClientSecrets =
                    //{
                    //    new Secret(clientSettings.ClientSecret.Sha256())
                    //},

                    AlwaysIncludeUserClaimsInIdToken = true,

                    RedirectUris = { clientSettings.RedirectUrl },
                    PostLogoutRedirectUris = { clientSettings.PostLogoutRedirectUrl },                

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        //"some.api.access"
                    }
                };
            }
        }
    }
}