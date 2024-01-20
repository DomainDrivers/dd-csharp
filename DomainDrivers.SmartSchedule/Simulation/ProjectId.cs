namespace DomainDrivers.SmartSchedule.Simulation;

public record ProjectId(Guid Id)
{
    public static ProjectId NewOne()
    {
        return new ProjectId(Guid.NewGuid());
    }
}