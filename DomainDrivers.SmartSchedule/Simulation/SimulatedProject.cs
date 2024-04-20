namespace DomainDrivers.SmartSchedule.Simulation;

public record SimulatedProject(ProjectId ProjectId, decimal Earnings, Demands MissingDemands)
{
    public bool AllDemandsSatisfied
    {
        get { return MissingDemands.All.Count == 0; }
    }
}