using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public class PlanningFacade
{
    private readonly IProjectRepository _projectRepository;
    private readonly StageParallelization _parallelization;
    private readonly PlanChosenResources _planChosenResourcesService;
    private readonly IEventsPublisher _eventsPublisher;
    private readonly TimeProvider _timeProvider;

    public PlanningFacade(IProjectRepository projectRepository, StageParallelization parallelization,
        PlanChosenResources resourcesPlanning, IEventsPublisher eventsPublisher, TimeProvider timeProvider)
    {
        _projectRepository = projectRepository;
        _parallelization = parallelization;
        _planChosenResourcesService = resourcesPlanning;
        _eventsPublisher = eventsPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<ProjectId> AddNewProject(string name, params Stage[] stages)
    {
        var parallelizedStages = _parallelization.Of(new HashSet<Stage>(stages));
        return await AddNewProject(name, parallelizedStages);
    }

    public async Task<ProjectId> AddNewProject(string name, ParallelStagesList parallelizedStages)
    {
        var project = new Project(name, parallelizedStages);
        await _projectRepository.Save(project);
        return project.Id;
    }

    public async Task DefineStartDate(ProjectId projectId, DateTime possibleStartDate)
    {
        var project = await _projectRepository.GetById(projectId);
        project.AddSchedule(possibleStartDate);
        await _projectRepository.Save(project);
    }

    public async Task DefineProjectStages(ProjectId projectId, params Stage[] stages)
    {
        var project = await _projectRepository.GetById(projectId);
        var parallelizedStages = _parallelization.Of(new HashSet<Stage>(stages));
        project.DefineStages(parallelizedStages);
        await _projectRepository.Save(project);
    }

    public async Task AddDemands(ProjectId projectId, Demands demands)
    {
        var project = await _projectRepository.GetById(projectId);
        project.AddDemands(demands);
        await _projectRepository.Save(project);
        await _eventsPublisher.Publish(new CapabilitiesDemanded(projectId, project.AllDemands,
            _timeProvider.GetUtcNow().DateTime));
    }

    public async Task DefineDemandsPerStage(ProjectId projectId, DemandsPerStage demandsPerStage)
    {
        var project = await _projectRepository.GetById(projectId);
        project.AddDemandsPerStage(demandsPerStage);
        await _projectRepository.Save(project);
        await _eventsPublisher.Publish(new CapabilitiesDemanded(projectId, project.AllDemands,
            _timeProvider.GetUtcNow().DateTime));
    }

    public async Task DefineResourcesWithinDates(ProjectId projectId, HashSet<ResourceId> chosenResources,
        TimeSlot timeBoundaries)
    {
        await _planChosenResourcesService.DefineResourcesWithinDates(projectId, chosenResources, timeBoundaries);
    }

    public async Task AdjustStagesToResourceAvailability(ProjectId projectId, TimeSlot timeBoundaries,
        params Stage[] stages)
    {
        await _planChosenResourcesService.AdjustStagesToResourceAvailability(projectId, timeBoundaries, stages);
    }

    public async Task PlanCriticalStageWithResource(ProjectId projectId, Stage criticalStage,
        ResourceId resourceId,
        TimeSlot stageTimeSlot)
    {
        var project = await _projectRepository.GetById(projectId);
        project.AddSchedule(criticalStage, stageTimeSlot);
        await _projectRepository.Save(project);
        await _eventsPublisher.Publish(new CriticalStagePlanned(projectId, stageTimeSlot, resourceId,
            _timeProvider.GetUtcNow().DateTime));
    }

    public async Task PlanCriticalStage(ProjectId projectId, Stage criticalStage, TimeSlot stageTimeSlot)
    {
        var project = await _projectRepository.GetById(projectId);
        project.AddSchedule(criticalStage, stageTimeSlot);
        await _projectRepository.Save(project);
        await _eventsPublisher.Publish(new CriticalStagePlanned(projectId, stageTimeSlot, null,
            _timeProvider.GetUtcNow().DateTime));
    }

    public async Task DefineManualSchedule(ProjectId projectId, Schedule schedule)
    {
        var project = await _projectRepository.GetById(projectId);
        project.AddSchedule(schedule);
        await _projectRepository.Save(project);
    }

    public TimeSpan DurationOf(params Stage[] stages)
    {
        return DurationCalculator.Calculate(stages.ToList());
    }

    public async Task<ProjectCard> Load(ProjectId projectId)
    {
        var project = await _projectRepository.GetById(projectId);
        return ToSummary(project);
    }

    public async Task<IList<ProjectCard>> LoadAll(HashSet<ProjectId> projectsIds)
    {
        return (await _projectRepository.FindAllByIdIn(projectsIds))
            .Select(project => ToSummary(project))
            .ToList();
    }

    public async Task<IList<ProjectCard>> LoadAll()
    {
        return (await _projectRepository.FindAll())
            .Select(project => ToSummary(project))
            .ToList();
    }

    private ProjectCard ToSummary(Project project)
    {
        return new ProjectCard(project.Id, project.Name, project.ParallelizedStages, project.AllDemands,
            project.Schedule, project.DemandsPerStage, project.ChosenResources);
    }
}