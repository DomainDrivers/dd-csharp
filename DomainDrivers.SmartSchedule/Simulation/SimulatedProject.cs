namespace DomainDrivers.SmartSchedule.Simulation;

public record SimulatedProject(ProjectId ProjectId, Func<decimal> Value, Demands MissingDemands)
{
    public decimal CalculateValue()
    {
        return Value();
    }
}