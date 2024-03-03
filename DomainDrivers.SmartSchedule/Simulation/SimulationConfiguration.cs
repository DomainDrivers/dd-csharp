namespace DomainDrivers.SmartSchedule.Simulation;

public static class SimulationConfiguration
{
    public static IServiceCollection AddSimulation(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<SimulationFacade>();
        return serviceCollection;
    }
}