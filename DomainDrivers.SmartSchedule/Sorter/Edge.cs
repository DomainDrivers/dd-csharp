namespace DomainDrivers.SmartSchedule.Sorter;

public record Edge(int Source, int Target)
{
    public override string ToString()
    {
        return $"({Source} -> {Target})";
    }
}
