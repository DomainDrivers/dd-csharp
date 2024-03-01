namespace DomainDrivers.SmartSchedule.Shared;

public static class SharedConfiguration
{
    public static IServiceCollection AddShared(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddTransient<TimeProvider>(_ => TimeProvider.System);
        serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SharedConfiguration).Assembly));
        serviceCollection.AddScoped<IEventsPublisher, EventsPublisher>();
        return serviceCollection;
    }
}