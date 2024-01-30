using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public class Project
{
    public ProjectId Id { get; private set; } = ProjectId.NewOne();

    private int _version;

    public string Name { get; private set; }

    public ParallelStagesList ParallelizedStages { get; private set; }

    public DemandsPerStage DemandsPerStage { get; private set; }

    public Demands AllDemands { get; private set; }

    public ChosenResources ChosenResources { get; private set; }

    public Schedule Schedule { get; private set; }

    public Project(string name, ParallelStagesList parallelizedStages)
    {
        Name = name;
        ParallelizedStages = parallelizedStages;
        AllDemands = Demands.None();
        Schedule = Schedule.None();
        ChosenResources = ChosenResources.None();
        DemandsPerStage = DemandsPerStage.Empty();
    }

    public void AddDemands(Demands demands)
    {
        AllDemands = AllDemands.Add(demands);
    }

    public void AddSchedule(DateTime possibleStartDate)
    {
        Schedule = Schedule.BasedOnStartDay(possibleStartDate, ParallelizedStages);
    }

    public void AddChosenResources(ChosenResources neededResources)
    {
        ChosenResources = neededResources;
    }

    public void AddDemandsPerStage(DemandsPerStage demandsPerStage)
    {
        DemandsPerStage = demandsPerStage;
        var uniqueDemands = demandsPerStage.Demands.Values
            .SelectMany(demands => demands.All)
            .ToHashSet();
        AddDemands(new Demands(uniqueDemands.ToList()));
    }

    public void AddSchedule(Stage criticalStage, TimeSlot stageTimeSlot)
    {
        Schedule = Schedule.BasedOnReferenceStageTimeSlot(criticalStage, stageTimeSlot, ParallelizedStages);
    }

    public void AddSchedule(Schedule schedule)
    {
        Schedule = schedule;
    }

    public void DefineStages(ParallelStagesList parallelizedStages)
    {
        ParallelizedStages = parallelizedStages;
    }
}