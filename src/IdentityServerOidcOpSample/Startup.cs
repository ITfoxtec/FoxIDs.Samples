// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServerOidcOpSample.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;

namespace IdentityServerOidcOpSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            var settings = services.BindConfig<IdentitySettings>(Configuration, nameof(IdentitySettings));
            var config = new Config(settings);

            var builder = services.AddIdentityServer(options =>
            {
                // To configure Identity Server to use the standard when emitting scopes into a JWT token.
                options.EmitScopesAsSpaceDelimitedStringInJwt = true;
            })
                .AddInMemoryIdentityResources(config.GetIdentityResources())
                .AddInMemoryApiResources(config.GetApiResources())
                .AddInMemoryApiScopes(config.GetApiScopes())
                .AddInMemoryClients(config.GetClients())
                .AddTestUsers(TestUsers.Users);

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
