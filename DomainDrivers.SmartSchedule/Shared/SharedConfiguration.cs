namespace DomainDrivers.SmartSchedule.Shared;

public static class SharedConfiguration
{
    public static IServiceCollection AddShared(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        return serviceCollection;
    }
}