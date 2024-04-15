using DomainDrivers.SmartSchedule.Planning.Parallelization;

namespace DomainDrivers.SmartSchedule.Planning;

public static class PlanningConfiguration
{
    public static IServiceCollection AddPlanning(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IProjectRepository, RedisProjectRepository>();
        serviceCollection.AddTransient<PlanChosenResources>();
        serviceCollection.AddTransient<StageParallelization>();
        serviceCollection.AddTransient<PlanningFacade>();
        return serviceCollection;
    }
}