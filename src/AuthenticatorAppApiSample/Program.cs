using AuthenticatorAppApiSample.Models;
using FoxIDs.SampleHelperLibrary.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.BindConfig<AppSettings>(builder.Configuration, nameof(AppSettings));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Authenticator App API Sample",
        Version = "v1"
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseWhen(_ => builder.Environment.IsDevelopment(), branch => branch.UseMiddleware<RawRequestLoggingMiddleware>());
app.MapControllers();

app.Run();
