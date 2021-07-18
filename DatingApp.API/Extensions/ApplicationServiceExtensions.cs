using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using DatingApp.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            #region Configure Settings from appsettings.json file
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            #endregion

            #region services DI
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            #endregion

            #region Action Filters
            services.AddScoped<LogUserActivity>();
            #endregion

            #region Configure AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            #endregion

            #region configure DbContext
            services.AddDbContext<DataContext>(options => {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });
            #endregion

            return services;
        }
    }
}