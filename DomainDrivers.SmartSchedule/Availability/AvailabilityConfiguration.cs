namespace DomainDrivers.SmartSchedule.Availability;

public static class AvailabilityConfiguration
{
    public static IServiceCollection AddAvailability(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IAvailabilityFacade, AvailabilityFacade>();
        serviceCollection.AddTransient<ResourceAvailabilityRepository>();
        serviceCollection.AddTransient<ResourceAvailabilityReadModel>();
        return serviceCollection;
    }
}