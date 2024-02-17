namespace DomainDrivers.SmartSchedule.Availability;

public static class AvailabilityConfiguration
{
    public static IServiceCollection AddAvailability(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<AvailabilityFacade>();
        serviceCollection.AddTransient<ResourceAvailabilityRepository>();
        return serviceCollection;
    }
}