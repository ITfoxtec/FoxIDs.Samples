using DirectoryConnectorApiSample.Models;
using DirectoryConnectorApiSample.Services;
using FoxIDs.SampleHelperLibrary.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.BindConfig<AppSettings>(builder.Configuration, nameof(AppSettings));
builder.Services.AddSingleton<DemoDirectoryStore>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new()
    {
        Title = "Directory Connector API Sample",
        Version = "v1"
    });
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
app.UseWhen(_ => builder.Environment.IsDevelopment(), branch => branch.UseMiddleware<RawRequestLoggingMiddleware>());
app.MapControllers();

app.Run();
