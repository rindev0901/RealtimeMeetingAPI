using RealtimeMeetingAPI.Interceptors;
using Microsoft.EntityFrameworkCore;
using RealtimeMeetingAPI.Data;
using AutoMapper;
using RealtimeMeetingAPI.Interfaces;
using RealtimeMeetingAPI.Services;
using RealtimeMeetingAPI.Helpers;
using RealtimeMeetingAPI.Hubs;

namespace RealtimeMeetingAPI.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static readonly string MyAllowSpecificOrigins = "_MyAllowSpecificOrigins";
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<SoftDeleteInterceptor>();
            services.AddSingleton<UserShareScreenTracker>();  // Add this line
            services.AddSingleton<PresenceTracker>();  // Add this line

            services.AddDbContext<ApplicationDbContext>((sp, options) => options
                    .UseNpgsql(config.GetConnectionString("DefaultConnection"))
                    .AddInterceptors(
                        sp.GetRequiredService<SoftDeleteInterceptor>()));

            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins("https://localhost:3000", "http://localhost:65477")
                                      .AllowAnyHeader()
                                      .AllowAnyMethod()
                                      .AllowCredentials();
                                  });
            });


            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });



            return services;
        }
    }
}
