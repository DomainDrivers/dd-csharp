using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;

namespace DomainDrivers.SmartSchedule.Allocation;

public record Projects(IDictionary<Guid, Project> ProjectsDictionary)
{
    public Projects Transfer(Guid projectFrom, Guid projectTo, AllocatedCapability capability, TimeSlot forSlot)
    {
        if (!ProjectsDictionary.TryGetValue(projectFrom, out var from) ||
            !ProjectsDictionary.TryGetValue(projectTo, out var to))
        {
            return this;
        }

        var removed = from.Remove(capability, forSlot);

        if (removed == null)
        {
            return this;
        }

        to.Add(new AllocatedCapability(removed.ResourceId, removed.Capability, forSlot));
        return new Projects(new Dictionary<Guid, Project>(ProjectsDictionary));
    }

    public IList<SimulatedProject> ToSimulatedProjects()
    {
        return ProjectsDictionary.Select(entry =>
                new SimulatedProject(ProjectId.From(entry.Key), () => entry.Value.Earnings,
                    GetMissingDemands(entry.Value)))
            .ToList();
    }

    private static DomainDrivers.SmartSchedule.Simulation.Demands GetMissingDemands(Project project)
    {
        var allDemands = project.MissingDemands();
        return new DomainDrivers.SmartSchedule.Simulation.Demands(
            allDemands.All
                .Select(demand => new DomainDrivers.SmartSchedule.Simulation.Demand(demand.Capability, demand.Slot))
                .ToList());
    }

    public virtual bool Equals(Projects? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ProjectsDictionary.DictionaryEqual(other.ProjectsDictionary);
    }

    public override int GetHashCode()
    {
        return ProjectsDictionary.CalculateHashCode();
    }
}