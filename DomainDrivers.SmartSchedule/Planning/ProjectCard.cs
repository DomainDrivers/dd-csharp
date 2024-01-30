using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;

namespace DomainDrivers.SmartSchedule.Planning;

public record ProjectCard(
    ProjectId ProjectId,
    string Name,
    ParallelStagesList ParallelizedStages,
    Demands Demands,
    Schedule Schedule,
    DemandsPerStage DemandsPerStage,
    ChosenResources NeededResources)
{
    public ProjectCard(ProjectId projectId, string name, ParallelStagesList parallelizedStages, Demands demands)
        : this(projectId, name, parallelizedStages, demands, Schedule.None(), DemandsPerStage.Empty(),
            ChosenResources.None())
    {
    }
}