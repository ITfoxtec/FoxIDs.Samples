using ExternalPasswordApiSample.Models;
using FoxIDs.SampleHelperLibrary.Middleware; 

var builder = WebApplication.CreateBuilder(args);

// Config binding
builder.Services.BindConfig<AppSettings>(builder.Configuration, nameof(AppSettings));

// Controllers + swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new()
    {
        Title = "External Password API Sample",
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
// Log raw HTTP requests in Development
app.UseWhen(_ => builder.Environment.IsDevelopment(), branch => branch.UseMiddleware<RawRequestLoggingMiddleware>()); 
app.MapControllers();

app.Run();
