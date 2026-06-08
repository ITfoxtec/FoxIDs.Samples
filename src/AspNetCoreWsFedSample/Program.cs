using AspNetCoreWsFedSample.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WsFederationSettings>(builder.Configuration.GetSection("WsFederation"));
var wsFederationSettings = builder.Configuration.GetSection("WsFederation").Get<WsFederationSettings>() ?? new WsFederationSettings();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "AspNetCoreWsFedSample";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddWsFederation(options =>
{
    options.MetadataAddress = wsFederationSettings.MetadataAddress;
    options.Wtrealm = wsFederationSettings.Wtrealm;
    options.Wreply = wsFederationSettings.Wreply;
    options.SignOutWreply = wsFederationSettings.SignOutWreply;
    options.CallbackPath = wsFederationSettings.CallbackPath;
    options.RemoteSignOutPath = wsFederationSettings.RemoteSignOutPath;
    options.RequireHttpsMetadata = wsFederationSettings.RequireHttpsMetadata;
    options.SaveTokens = true;
});

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
