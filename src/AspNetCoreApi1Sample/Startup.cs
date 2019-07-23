using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AspNetCoreApi1Sample.Models;
using AspNetCoreApi1Sample.Policys;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AspNetCoreApi1Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var identitySettings = services.BindConfig<IdentitySettings>(Configuration, nameof(IdentitySettings));

            services.AddMvc()
               .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
               .AddJsonOptions(options =>
               {
                   options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                   options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
               });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = identitySettings.Authority;
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
