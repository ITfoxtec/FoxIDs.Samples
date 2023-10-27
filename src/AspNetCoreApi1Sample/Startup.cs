using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AspNetCoreApi1Sample.Models;
using AspNetCoreApi1Sample.Policys;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using ITfoxtec.Identity.Helpers;
using ITfoxtec.Identity.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace AspNetCoreApi1Sample
{
    public class Startup
    {
        public static IWebHostEnvironment AppEnvironment { get; private set; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            AppEnvironment = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var identitySettings = services.BindConfig<IdentitySettings>(Configuration, nameof(IdentitySettings));
            services.BindConfig<AppSettings>(Configuration, nameof(AppSettings));

            services.AddSingleton((serviceProvider) =>
            {
                var settings = serviceProvider.GetService<IdentitySettings>();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

                return new OidcDiscoveryHandler(httpClientFactory, UrlCombine.Combine(identitySettings.AuthorityWithoutUpParty, IdentityConstants.OidcDiscovery.Path));
            });

            services.AddTransient<TokenExecuteHelper>();

            services.AddHttpClient();
            services.AddHttpContextAccessor();

            services.AddCors();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = identitySettings.AuthorityWithoutUpParty;
                    options.Audience = identitySettings.ResourceId;

                    options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Subject;
                    options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = async (context) =>
                        {
                            await Task.FromResult(string.Empty);
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                Api1SomeAccessScopeAuthorizeAttribute.AddPolicy(options);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization);
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
