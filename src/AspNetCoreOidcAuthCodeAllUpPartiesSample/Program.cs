using AspNetCoreOidcAuthCodeAllUpPartiesSample.Models;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using ITfoxtec.Identity.Util;
using AspNetCoreOidcAuthCodeAllUpPartiesSample.Identity;
using ITfoxtec.Identity.Helpers;
using Microsoft.IdentityModel.Logging;
using FoxIDs.SampleHelperLibrary.Identity;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem

builder.Services.AddApplicationInsightsTelemetry();

var identitySettings = builder.Services.BindConfig<IdentitySettings>(builder.Configuration, nameof(IdentitySettings));
builder.Services.BindConfig<AppSettings>(builder.Configuration, nameof(AppSettings));

builder.Services.AddSingleton((serviceProvider) =>
{
    var settings = serviceProvider.GetService<IdentitySettings>();
    var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

    return new OidcDiscoveryHandler(httpClientFactory, UrlCombine.Combine(settings.FoxIDsAuthority, IdentityConstants.OidcDiscovery.Path));
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Events.OnValidatePrincipal = async (context) =>
        {
            var logoutMemoryCache = context.HttpContext.RequestServices.GetService<LogoutMemoryCache>();
            var sessionId = context.Principal.Claims.Where(c => c.Type == JwtClaimTypes.SessionId).Select(c => c.Value).FirstOrDefault();
            foreach(var item in logoutMemoryCache.List)
            {
                if(sessionId == item)
                {
                    logoutMemoryCache.Remove(item);
                    // Handle FrontChannelLogout
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync();
                    return;
                }
            }             

            try
            {
                var expiresUtc = DateTimeOffset.Parse(context.Properties.GetTokenValue("expires_at"));

                // Tokens expires 30 seconds before actual expiration time.
                if (expiresUtc < DateTimeOffset.UtcNow.AddSeconds(30))
                {
                    var tokenResponse = await RefreshTokenHandler.ResolveRefreshToken(context, identitySettings);

                    context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokenResponse.AccessToken);
                    context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.IdToken, tokenResponse.IdToken);
                    if (!tokenResponse.RefreshToken.IsNullOrEmpty())
                    {
                        context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokenResponse.RefreshToken);
                    }
                    else
                    {
                        context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, context.Properties.GetTokenValue(OpenIdConnectParameterNames.RefreshToken));
                    }
                    context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.TokenType, tokenResponse.TokenType);

                    var newExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn.HasValue ? tokenResponse.ExpiresIn.Value : 30);
                    context.Properties.UpdateTokenValue("expires_at", newExpiresUtc.ToString("o", CultureInfo.InvariantCulture));

                    // Cookie should be renewed.
                    context.ShouldRenew = true;
                }
            }
            catch
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync();
            }
        };
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = identitySettings.FoxIDsAuthority;
        options.ClientId = identitySettings.ClientId;
        options.ClientSecret = identitySettings.ClientSecret;

        options.ResponseType = OpenIdConnectResponseType.Code;

        options.SaveTokens = true;
        // False to support refresh token renewal.
        options.UseTokenLifetime = false;

        // To show the acr claim in the User.Claims collection
        options.ClaimActions.Remove("acr");

        // Scope to the application it self, used to do token exchange.
        options.Scope.Add(identitySettings.DownParty);
        options.Scope.Add(identitySettings.RequestApi1Scope);
        options.Scope.Add("offline_access");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Subject;
        options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

        options.Events.OnRedirectToIdentityProvider = async (context) =>
        {
            // To require MFA
            //context.ProtocolMessage.AcrValues = "urn:foxids:mfa";
            // Request a language on login
            //context.ProtocolMessage.UiLocales = "fr";
            await Task.FromResult(string.Empty);
        };
        options.Events.OnRedirectToIdentityProviderForSignOut = async (context) =>
        {
            // Request a language on logout
            //context.ProtocolMessage.UiLocales = "fr";
            await Task.FromResult(string.Empty);
        };
        options.Events.OnTokenResponseReceived = async (context) =>
        {
            if (!context.TokenEndpointResponse.Error.IsNullOrEmpty())
            {
                throw new Exception($"Token response error. {context.TokenEndpointResponse.Error}, {context.TokenEndpointResponse.ErrorDescription} ");
            }
            await Task.FromResult(string.Empty);
        };
        options.Events.OnRemoteFailure = async (context) =>
        {
            await Task.FromResult(string.Empty);
        };
    });

builder.Services.AddTransient<TokenExecuteHelper>();
builder.Services.AddSingleton<LogoutMemoryCache>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
