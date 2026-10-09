using ExternalClaimsApiSample.Models;
using ExternalClaimsApiSample.Models.Api;
using ExternalClaimsApiSample.Serialization;
using FoxIDs.SampleHelperLibrary.Middleware;
using FoxIDs.SampleHelperLibrary.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Config binding
var appSettings = builder.Services.BindConfig<AppSettings>(builder.Configuration, nameof(AppSettings));
Validator.ValidateObject(appSettings, new ValidationContext(appSettings), validateAllProperties: true);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        if (appSettings.ClaimsFormat == ClaimsFormats.Properties)
        {
            options.JsonSerializerOptions.Converters.Add(new ClaimsRequestPropertiesConverter());
            options.JsonSerializerOptions.Converters.Add(new ClaimsResponsePropertiesConverter());
        }
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new()
    {
        Title = "External Claims API Sample",
        Version = "v1",
        Description = $"Claims format: {appSettings.ClaimsFormat}. Set AppSettings:ClaimsFormat and select the same format in FoxIDs."
    });
    if (appSettings.ClaimsFormat == ClaimsFormats.Properties)
    {
        o.MapType<ClaimsRequest>(ClaimPropertiesSchema.Create);
        o.MapType<ClaimsResponse>(ClaimPropertiesSchema.Create);
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
// Log raw HTTP requests in Development
app.UseWhen(_ => builder.Environment.IsDevelopment(), branch => branch.UseMiddleware<RawRequestLoggingMiddleware>());
app.MapControllers();

app.Run();

public partial class Program { }
