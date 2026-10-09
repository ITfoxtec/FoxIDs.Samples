using DirectoryConnectorApiSample.Models;
using DirectoryConnectorApiSample.Models.Api;
using DirectoryConnectorApiSample.Serialization;
using DirectoryConnectorApiSample.Services;
using FoxIDs.SampleHelperLibrary.Models;
using Microsoft.AspNetCore.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Services.BindConfig<AppSettings>(builder.Configuration, nameof(AppSettings));
Validator.ValidateObject(appSettings, new ValidationContext(appSettings), validateAllProperties: true);
builder.Services.AddSingleton<DemoDirectoryStore>();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("en")
        .AddSupportedCultures("en", "da")
        .AddSupportedUICultures("en", "da");
    options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        if (appSettings.ClaimsFormat == ClaimsFormats.Properties)
        {
            options.JsonSerializerOptions.Converters.Add(new ClaimPropertiesConverter());
        }
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new()
    {
        Title = "Directory Connector API Sample",
        Version = "v1",
        Description = $"Claims format: {appSettings.ClaimsFormat}. Set AppSettings:ClaimsFormat and select the same format in FoxIDs."
    });
    if (appSettings.ClaimsFormat == ClaimsFormats.Properties)
    {
        o.MapType<IEnumerable<ClaimValue>>(ClaimPropertiesSchema.Create);
    }
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        o.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRequestLocalization();
app.MapControllers();

app.Run();

public partial class Program { }
