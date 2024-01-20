namespace DomainDrivers.SmartSchedule.Simulation;

public class SimulationFacade
{
    public Result WhichProjectWithMissingDemandsIsMostProfitableToAllocateResourcesTo(List<SimulatedProject> projects,
        SimulatedCapabilities totalCapability)
    {
        return new Result(0.0, new List<SimulatedProject>(),
            new Dictionary<SimulatedProject, ISet<AvailableResourceCapability>>());
    }
}