using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Simulation;

public record Result(double Profit, IList<SimulatedProject> ChosenProjects,
    IDictionary<SimulatedProject, ISet<AvailableResourceCapability>> ResourcesAllocatedToProjects)
{
    public virtual bool Equals(Result? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Profit == other.Profit
               && ChosenProjects.SequenceEqual(other.ChosenProjects)
               && ResourcesAllocatedToProjects.DictionaryEqual(other.ResourcesAllocatedToProjects,
                   EqualityComparer<ISet<AvailableResourceCapability>>.Create((x, y) => x!.SetEquals(y!)));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Profit, ChosenProjects.CalculateHashCode(), ResourcesAllocatedToProjects.CalculateHashCode());
    }

    public override string ToString()
    {
        return $"Result{{profit={Profit}, items={ChosenProjects.ToCollectionString()}}}";
    }
}