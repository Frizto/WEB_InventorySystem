using InventorySystem.Application.Extension.Identity;
using InventorySystem.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.Infrastructure.DependencyInjection;
public static class ServiceContainer
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

        }).AddIdentityCookies();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            //options.SignIn.RequireConfirmedEmail = true;
            //options.User.RequireUniqueEmail = true;
            //options.Password.RequireDigit = true;
            //options.Password.RequireLowercase = true;
            //options.Password.RequireUppercase = true;
            //options.Password.RequireNonAlphanumeric = true;
            //options.Password.RequiredLength = 8;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthorizationBuilder()
            .AddPolicy("AdministrationPolicy", adp =>
        {
            adp.RequireAuthenticatedUser();
            adp.RequireRole("Admin", "Manager");
        })
            .AddPolicy("UserPolicy", adp =>
        {
            adp.RequireAuthenticatedUser();
            adp.RequireRole("User");
        });

        //services.AddScoped<IUserClaimsRepository, UserClaimsRepository>();

        return services;
    }
}
