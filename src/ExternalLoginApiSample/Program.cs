using ExternalLoginApiSample.Models;
using FoxIDs.SampleHelperLibrary.Infrastructure.Hosting;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.BindConfig<AppSettings>(builder.Configuration, nameof(AppSettings));

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<ProxyHeadersMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
