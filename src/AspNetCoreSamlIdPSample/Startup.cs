using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2;
using FoxIDs.SampleHelperLibrary;
using AspNetCoreSamlIdPSample.Models;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using System;
using System.Linq;
using FoxIDs.SampleHelperLibrary.Repository;

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

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<Settings>(Configuration.GetSection("Settings"));
            services.Configure<Settings>(settings =>
            {
                foreach(var rp in settings.RelyingParties)
                {
                    var entityDescriptor = new EntityDescriptor();
                    entityDescriptor.ReadSPSsoDescriptorFromUrl(new Uri(rp.SpMetadata));
                    if (entityDescriptor.SPSsoDescriptor != null)
                    {
                        rp.Issuer = entityDescriptor.EntityId;
                        rp.SingleSignOnDestination = entityDescriptor.SPSsoDescriptor.AssertionConsumerServices.First().Location;

                        var singleLogoutService = entityDescriptor.SPSsoDescriptor.SingleLogoutServices.First();
                        rp.SingleLogoutResponseDestination = singleLogoutService.ResponseLocation ?? singleLogoutService.Location;

                        rp.SignatureValidationCertificate = entityDescriptor.SPSsoDescriptor.SigningCertificates.First();
                    }
                    else
                    {
                        throw new Exception("IdPSsoDescriptor not loaded from metadata.");
                    }
                }
            });

            services.Configure<Saml2Configuration>(Configuration.GetSection("Saml2"));
            services.Configure<Saml2Configuration>(saml2Configuration =>
            {
                saml2Configuration.SigningCertificate = TestCertificate.GetSelfSignedCertificate(AppEnvironment.ContentRootPath, "test-sign-cert");

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
