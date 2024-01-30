namespace DomainDrivers.SmartSchedule.Planning;

public record ProjectId(Guid Id)
{
    public static ProjectId NewOne()
    {
        return new ProjectId(Guid.NewGuid());
    }
}