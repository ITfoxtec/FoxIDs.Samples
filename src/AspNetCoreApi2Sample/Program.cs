using ITfoxtec.Identity;
using AspNetCoreApi2Sample.Models;
using AspNetCoreApi2Sample.Policies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json.Serialization;
using Microsoft.Net.Http.Headers;
using Microsoft.IdentityModel.Logging;
using FoxIDs.SampleHelperLibrary.Infrastructure.Hosting;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem

builder.Services.AddApplicationInsightsTelemetry();

var identitySettings = builder.Services.BindConfig<IdentitySettings>(builder.Configuration, nameof(IdentitySettings));

builder.Services.AddCors();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identitySettings.FoxIDsAuthority;
        options.Audience = identitySettings.ResourceId;

        options.MapInboundClaims = false;
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

builder.Services.AddAuthorization(Api2SomeAccessScopeAuthorizeAttribute.AddPolicy);


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseMiddleware<ProxyHeadersMiddleware>();

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