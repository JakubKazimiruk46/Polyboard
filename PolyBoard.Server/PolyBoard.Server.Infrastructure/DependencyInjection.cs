using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;
using PolyBoard.Server.Infrastructure.Authentication;
using PolyBoard.Server.Infrastructure.Data;
using PolyBoard.Server.Infrastructure.Repositories;


namespace PolyBoard.Server.Infrastructure;

public static class DependencyInjection
{
    public  static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRepository<User>, Repository<User>>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        //Mediator Pattern Setup
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            
        //Add DbContext
        services.AddDbContext<PostgresDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        //Allow DateTime mapping in Postgres
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
            
        //Setup Identity
        services
            .AddIdentityCore<User>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<PostgresDbContext>();

        return services;
    }
}