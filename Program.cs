using Microsoft.OpenApi.Models;
using RealtimeMeetingAPI.Extensions;
using RealtimeMeetingAPI.Hubs;
using RealtimeMeetingAPI.Middlwares;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddApplicationServices(configuration);

builder.Services.AddIdentityServices(configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RealtimeMeetingAPI", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(ApplicationServiceExtensions.MyAllowSpecificOrigins);
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<PresenceHub>("hubs/presence");

app.MapControllers();

app.Run();
