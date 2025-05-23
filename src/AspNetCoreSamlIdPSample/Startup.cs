﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using AspNetCoreSamlIdPSample.Models;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using System;
using System.Linq;
using FoxIDs.SampleHelperLibrary.Repository;
using System.Security.Cryptography.X509Certificates;
using ITfoxtec.Identity;
using System.IO;
using ITfoxtec.Identity.Saml2.Util;
using Microsoft.IdentityModel.Logging;
using FoxIDs.SampleHelperLibrary.Infrastructure.Hosting;

namespace AspNetCoreSamlIdPSample
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            AppEnvironment = env;
            Configuration = configuration;
        }

        private IWebHostEnvironment AppEnvironment { get; set; }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem

            services.AddApplicationInsightsTelemetry();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<Settings>(Configuration.GetSection("Settings"));

            // Add LibrarySettings for ProxyHeadersMiddleware
            services.BindConfig<Settings>(Configuration, nameof(Settings));

            services.Configure<Saml2ConfigurationIdP>(Configuration.GetSection("Saml2"));
            services.Configure<Saml2ConfigurationIdP>(saml2Configuration =>
            {
                if (!saml2Configuration.TokenExchangeClientCertificateThumbprint.IsNullOrEmpty())
                {
                    saml2Configuration.SigningCertificate = CertificateUtil.Load(StoreName.My, StoreLocation.CurrentUser, X509FindType.FindByThumbprint, saml2Configuration.TokenExchangeClientCertificateThumbprint);
                }
                else
                {
                    var parth = Path.Combine(AppEnvironment.ContentRootPath, "Certificates");
                    try
                    {
                        if (saml2Configuration.TokenExchangeClientCertificatePassword.IsNullOrEmpty())
                        {
                            saml2Configuration.SigningCertificate = CertificateUtil.Load(Path.Combine(parth, saml2Configuration.TokenExchangeClientCertificateFile), loadPkcs12: true);
                        }
                        else
                        {
                            saml2Configuration.SigningCertificate = CertificateUtil.Load(Path.Combine(parth, saml2Configuration.TokenExchangeClientCertificateFile), saml2Configuration.TokenExchangeClientCertificatePassword);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Load certificate '{saml2Configuration.TokenExchangeClientCertificateFile}' error, path '{parth}'.", ex);
                    }
                }

                saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);
            });

            services.AddTransient<IdPSessionCookieRepository>();

            services.AddSaml2();

            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseMiddleware<ProxyHeadersMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseSaml2();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
