using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace PatientInfo.Api.Sdk;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures the PatientInfo API client to the service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the client to.</param>
    /// <param name="configuration">The application configuration, used to retrieve the API base URL.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the API base URL is not configured.</exception>
    public static IServiceCollection AddPatientInfoApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        // Get the base URL from configuration (e.g., appsettings.json)
        var baseUrl = configuration["PatientInfoApi:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "The Base URL for the Patient Info API is not configured. " +
                "Please set 'PatientInfoApi:BaseUrl' in your configuration."
            );
        }

        // Add the Refit client to the dependency injection container
        services.AddRefitClient<IPatientInfoApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));

        return services;
    }
}