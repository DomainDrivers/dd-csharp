namespace DomainDrivers.SmartSchedule.Resource;

public static class ResourceConfiguration
{
    public static IServiceCollection AddResource(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<ResourceFacade>();
        return serviceCollection;
    }
}