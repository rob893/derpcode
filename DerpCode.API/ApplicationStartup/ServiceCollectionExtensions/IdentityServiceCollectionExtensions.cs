using System;
using DerpCode.API.Data;
using DerpCode.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = services.AddIdentityCore<User>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequiredLength = 8;
            opt.Password.RequireNonAlphanumeric = true;
            opt.Password.RequireUppercase = false;
            opt.User.RequireUniqueEmail = true;
        }).AddDefaultTokenProviders();

        builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
        builder.AddEntityFrameworkStores<DataContext>();
        builder.AddRoleValidator<RoleValidator<Role>>();
        builder.AddRoleManager<RoleManager<Role>>();
        builder.AddSignInManager<SignInManager<User>>();

        return services;
    }
}