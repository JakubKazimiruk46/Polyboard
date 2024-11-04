using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace PolyBoard.Server.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //Setup Mediator Pattern
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblies(assemblies));
        //Setup FluentValidation
        services.AddValidatorsFromAssemblies(assemblies);
        
        return services;
    }
}