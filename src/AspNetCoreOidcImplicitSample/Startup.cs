using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AspNetCoreOidcImplicitSample.Models;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using FoxIDs.SampleHelperLibrary.Models;
using FoxIDs.SampleHelperLibrary.Repository;
using Microsoft.Extensions.Hosting;

namespace AspNetCoreOidcImplicitSample
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
            IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddTransient<IdPSelectionCookieRepository>();

            var identitySettings = services.BindConfig<IdentitySettings>(Configuration, nameof(IdentitySettings));

            services.AddControllersWithViews();
            services.AddHttpContextAccessor();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    // Required to support Front channel logout
                    options.Cookie.SameSite = SameSiteMode.None;
                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = identitySettings.Authority;
                    options.ClientId = identitySettings.ClientId;

                    options.ResponseType = "id_token token";
                    //options.ResponseType = "id_token";

                    options.SaveTokens = true;

                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Subject;
                    options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

                    options.Events.OnRedirectToIdentityProvider = async (context) =>
                    {                      
                        var loginType = ReadLoginType(context.Properties);
                        SetIssuerAddress(context, loginType);

                        await Task.FromResult(string.Empty);
                    };
                    options.Events.OnRedirectToIdentityProviderForSignOut = async (context) =>
                    {
                        var loginType = await GetSelectedLoginType(context.HttpContext);
                        SetIssuerAddress(context, loginType);

                        await Task.FromResult(string.Empty);
                    };
                    options.Events.OnTokenValidated = async (context) =>
                    {
                        var idPSelectionCookieRepository = context.HttpContext.RequestServices.GetService<IdPSelectionCookieRepository>();
                        var loginType = ReadLoginType(context.Properties);
                        await idPSelectionCookieRepository.SaveAsync(loginType.ToString());

                        await Task.FromResult(string.Empty);
                    };
                    options.Events.OnRemoteFailure = async (context) =>
                    {

                        await Task.FromResult(string.Empty);
                    };
                });
        }

        private static LoginType ReadLoginType(AuthenticationProperties properties)
        {
            LoginType loginType;
            if (!properties.Items.ContainsKey(Constants.StateLoginType) || !Enum.TryParse(properties.Items[Constants.StateLoginType], true, out loginType))
            {
                loginType = LoginType.FoxIDsLogin;
            }

            return loginType;
        }

        private async Task<LoginType> GetSelectedLoginType(HttpContext httpContext)
        {
            var idPSelectionCookieRepository = httpContext.RequestServices.GetService<IdPSelectionCookieRepository>();

            var loginTypeValue = await idPSelectionCookieRepository.GetAsync();
            if (!string.IsNullOrEmpty(loginTypeValue))
            {
                LoginType loginType;
                if (Enum.TryParse(loginTypeValue, true, out loginType))
                {
                    return loginType;
                }
            }

            throw new InvalidOperationException("Unable to read Login Type from IdP session cookie.");
        }

        private void SetIssuerAddress(RedirectContext context, LoginType loginType)
        {
            if (context.ProtocolMessage.RequestType != OpenIdConnectRequestType.Token)
            {
                var settings = context.HttpContext.RequestServices.GetService<IdentitySettings>();

                var upParty = GetUpParty(settings, loginType);
                context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress.Replace($"/{settings.DownParty}/", $"/{settings.DownParty}({upParty})/");
            }
        }

        private string GetUpParty(IdentitySettings settings, LoginType loginType)
        {
            switch (loginType)
            {
                case LoginType.FoxIDsLogin:
                    return settings.FoxIDsLoginUpParty;
                case LoginType.ParallelFoxIDs:
                    return settings.ParallelFoxIDsUpParty;
                case LoginType.IdentityServer:
                    return settings.IdentityServerUpParty;
                case LoginType.SamlIdPSample:
                    return settings.SamlIdPSampleUpParty;
                case LoginType.SamlIdPAdfs:
                    return settings.SamlIdPAdfsUpParty;
                default:
                    throw new NotImplementedException("LoginType not implemented.");
            }
        }

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

            app.UseAuthentication();
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
