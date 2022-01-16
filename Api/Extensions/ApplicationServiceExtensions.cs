using Business.Services.Abstract;
using Business.Services.Concrete.EntityFramework;
using Core.UnitOfWork;
using Core.UnitOfWork.ORMS;
using Core.Utilities.Security.Jwt;
using DataAccess.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions
{
    public static class ApplicationServiceExtensions
    {

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddScoped<DbContext, ApplicationDbContext>();
            services.AddScoped<ITokenHelper, JwtHelper>();

            services.AddDbContext<ApplicationDbContext>(o =>
            {                
                o.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    x => { x.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name); });
            });

            return services;
        }


    }
}
