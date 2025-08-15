using ExternalPasswordApiSample.Models;

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

app.MapControllers();

app.Run();
