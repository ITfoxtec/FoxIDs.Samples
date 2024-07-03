using ExternalLoginApiSample.Models;

    var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.BindConfig<AppSettings>(builder.Configuration, nameof(AppSettings));

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
