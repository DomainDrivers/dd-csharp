namespace DomainDrivers.SmartSchedule.Optimization;

public static class OptimizationConfiguration
{
    public static IServiceCollection AddOptimization(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<OptimizationFacade>();
        return serviceCollection;
    }
}