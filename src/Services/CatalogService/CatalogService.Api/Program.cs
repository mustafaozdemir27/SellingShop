using CatalogService.Api.Extensions;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = typeof(Program).Assembly.GetName().Name,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "Pics",
    EnvironmentName = Environments.Development // -> Staging -> Production
});

// Add services to the container.
builder.Services.AddControllers();

// OpenAPI (Swagger) configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Catalog Service API",
        Version = "v1"
    });
});

builder.Services.Configure<CatalogSettings>(builder.Configuration.GetSection(nameof(CatalogSettings)));
builder.Services.ConfigureDbContext(builder.Configuration);

builder.Services.ConfigureConsul(builder.Configuration);

var app = builder.Build();

// DB Migrate & Seed
app.MigrateDbContext<CatalogContext>((context, services) =>
{
    var env = services.GetService<IWebHostEnvironment>();
    var logger = services.GetService<ILogger<CatalogContextSeed>>();

    new CatalogContextSeed()
        .SeedAsync(context, env, logger)
        .Wait();
});

// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service API V1");
    });
}

app.UseAuthorization();
app.MapControllers();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
app.RegisterWithConsul(lifetime);

app.Run();
