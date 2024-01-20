using DomainDrivers.SmartSchedule.Simulation;

namespace DomainDrivers.SmartSchedule.Tests.Simulation;

public class SimulatedProjectsBuilder
{
    private ProjectId? _currentId;
    private readonly IList<ProjectId> _simulatedProjects = new List<ProjectId>();
    private readonly IDictionary<ProjectId, Demands> _simulatedDemands = new Dictionary<ProjectId, Demands>();
    private readonly IDictionary<ProjectId, Func<decimal>> _values = new Dictionary<ProjectId, Func<decimal>>();

    public SimulatedProjectsBuilder WithProject(ProjectId id)
    {
        _currentId = id;
        _simulatedProjects.Add(id);
        return this;
    }

    public SimulatedProjectsBuilder ThatRequires(params Demand[] demands)
    {
        _simulatedDemands[_currentId!] = Demands.Of(demands);
        return this;
    }

    public SimulatedProjectsBuilder ThatCanEarn(decimal earnings)
    {
        _values[_currentId!] = () => earnings;
        return this;
    }

    public SimulatedProjectsBuilder ThatCanGenerateReputationLoss(int factor)
    {
        _values[_currentId!] = () => factor;
        return this;
    }

    public List<SimulatedProject> Build()
    {
        return _simulatedProjects
            .Select(id => new SimulatedProject(id, _values[id], _simulatedDemands[id]))
            .ToList();
    }
}