using AspNetCoreApiOAuthTwoIdPsSample.Identity;
using AspNetCoreApiOAuthTwoIdPsSample.Models;
using AspNetCoreApiOAuthTwoIdPsSample.Policys;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var identitySettings = builder.Services.BindConfig<IdentitySettings>(builder.Configuration, nameof(IdentitySettings));

// True to show token validation exception details.
IdentityModelEventSource.ShowPII = true;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services
    .AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme + "1", options =>
    {
        options.Authority = identitySettings.Authority1;
        options.Audience = identitySettings.ResourceId1;

        options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Subject;
        options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    identity.AddClaim(new Claim(ApiJwtClaimTypes.IdP, "idp1"));
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = async (context) =>
            {
                // The error "IDX10501: Signature validation failed. Unable to match key..." is caused by the system checking
                // each AddJwtBearer in turn until it gets a match. The error can usually be ignored.
                //Console.WriteLine(context.Exception.ToString());
                await Task.FromResult(string.Empty);
            }
        };
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme + "2", options =>
    {
        options.Authority = identitySettings.Authority2;
        options.Audience = identitySettings.ResourceId2;

        options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Subject;
        options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    identity.AddClaim(new Claim(ApiJwtClaimTypes.IdP, "idp2"));
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = async (context) =>
            {
                //Console.WriteLine(context.Exception.ToString());
                await Task.FromResult(string.Empty);
            }
        };
    });

builder.Services
    .AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme + "1", JwtBearerDefaults.AuthenticationScheme + "2")
            .AddRequirements(new IdPWithScopesAuthorizationRequirement(new IdPWithScopes[]
                {
                    new IdPWithScopes { IdP = "idp1", Scopes = new string[] { "aspnetapi_oauth_twoidps_sample:some_access" } },
                    new IdPWithScopes { IdP = "idp2", Scopes = new string[] { "some_idp2_scope_access" } }
                }))
            .Build();
    });

builder.Services.AddCors();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization);
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
