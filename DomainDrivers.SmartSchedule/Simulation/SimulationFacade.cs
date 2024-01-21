using DomainDrivers.SmartSchedule.Optimization;

namespace DomainDrivers.SmartSchedule.Simulation;

public class SimulationFacade
{
    private readonly OptimizationFacade _optimizationFacade;

    public SimulationFacade(OptimizationFacade optimizationFacade)
    {
        _optimizationFacade = optimizationFacade;
    }

    public Result WhichProjectWithMissingDemandsIsMostProfitableToAllocateResourcesTo(
        IList<SimulatedProject> projectsSimulations, SimulatedCapabilities totalCapability)
    {
        return _optimizationFacade.Calculate(ToItems(projectsSimulations), ToCapacity(totalCapability));
    }

    private TotalCapacity ToCapacity(SimulatedCapabilities simulatedCapabilities)
    {
        var capabilities = simulatedCapabilities.Capabilities;
        var capacityDimensions = new List<ICapacityDimension>(capabilities);
        return new TotalCapacity(capacityDimensions);
    }

    private IList<Item> ToItems(IList<SimulatedProject> projectsSimulations)
    {
        return projectsSimulations
            .Select(project => ToItem(project))
            .ToList();
    }

    private Item ToItem(SimulatedProject simulatedProject)
    {
        var missingDemands = simulatedProject.MissingDemands.All;
        IList<IWeightDimension> weights = new List<IWeightDimension>(missingDemands);
        return new Item(simulatedProject.ProjectId.ToString(),
            decimal.ToDouble(simulatedProject.Earnings), new TotalWeight(weights));
    }
}