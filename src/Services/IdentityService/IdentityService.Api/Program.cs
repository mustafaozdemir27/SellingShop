using IdentityService.Api.Application.Services;
using IdentityService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddScoped<IIdentityManagementService, IdentityManagementService>();

builder.Services.ConfigureConsul(builder.Configuration);

// Configure Kestrel to listen on all IP addresses
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5005);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
app.RegisterWithConsul(lifetime);

app.Run();
