using Application;
using Application.Behaviors;
using FluentValidation;
using MediatR;

namespace App.Configurations;

public class ApplicationServiceInstaller : IServiceInstaller
{
    /// <summary>
    /// Configures the application services.
    /// </summary>
    /// <param name="services">The collection of services to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        // Add MediatR services for handling commands and queries
        services.AddMediatR(cfg =>
        {
            // Register handlers from the specified assembly
            cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);
        });


        // Add caching behavior FIRST to check cache before executing request
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachePipelineBehavior<,>));

        // Add validation behavior NEXT to validate input data
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

        // Add logging behavior LAST to log execution
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        
        // Add FluentValidation validators from the specified assembly
        services.AddValidatorsFromAssembly(
            AssemblyReference.Assembly,
            includeInternalTypes: true);
    }
}