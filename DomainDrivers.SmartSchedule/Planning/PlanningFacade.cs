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
    private readonly IUnitOfWork _unitOfWork;

    public PlanningFacade(IProjectRepository projectRepository, StageParallelization parallelization,
        PlanChosenResources resourcesPlanning, IEventsPublisher eventsPublisher, TimeProvider timeProvider,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _parallelization = parallelization;
        _planChosenResourcesService = resourcesPlanning;
        _eventsPublisher = eventsPublisher;
        _timeProvider = timeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectId> AddNewProject(string name, params Stage[] stages)
    {
        var parallelizedStages = _parallelization.Of(new HashSet<Stage>(stages));
        return await AddNewProject(name, parallelizedStages);
    }

    public async Task<ProjectId> AddNewProject(string name, ParallelStagesList parallelizedStages)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var project = new Project(name, parallelizedStages);
            await _projectRepository.Add(project);
            return project.Id;
        });
    }

    public async Task DefineStartDate(ProjectId projectId, DateTime possibleStartDate)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            project.AddSchedule(possibleStartDate);
            await _projectRepository.Update(project);
        });
    }

    public async Task DefineProjectStages(ProjectId projectId, params Stage[] stages)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            var parallelizedStages = _parallelization.Of(new HashSet<Stage>(stages));
            project.DefineStages(parallelizedStages);
            await _projectRepository.Update(project);
        });
    }

    public async Task AddDemands(ProjectId projectId, Demands demands)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            project.AddDemands(demands);
            await _projectRepository.Update(project);
            await _eventsPublisher.Publish(new CapabilitiesDemanded(projectId, project.AllDemands,
                _timeProvider.GetUtcNow().DateTime));
        });
    }

    public async Task DefineDemandsPerStage(ProjectId projectId, DemandsPerStage demandsPerStage)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            project.AddDemandsPerStage(demandsPerStage);
            await _projectRepository.Update(project);
            await _eventsPublisher.Publish(new CapabilitiesDemanded(projectId, project.AllDemands,
                _timeProvider.GetUtcNow().DateTime));
        });
    }

    public async Task DefineResourcesWithinDates(ProjectId projectId, HashSet<ResourceId> chosenResources,
        TimeSlot timeBoundaries)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            await _planChosenResourcesService.DefineResourcesWithinDates(projectId, chosenResources, timeBoundaries);
        });
    }

    public async Task AdjustStagesToResourceAvailability(ProjectId projectId, TimeSlot timeBoundaries,
        params Stage[] stages)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            await _planChosenResourcesService.AdjustStagesToResourceAvailability(projectId, timeBoundaries, stages);
        });
    }

    public async Task PlanCriticalStageWithResource(ProjectId projectId, Stage criticalStage,
        ResourceId resourceId,
        TimeSlot stageTimeSlot)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            project.AddSchedule(criticalStage, stageTimeSlot);
            await _projectRepository.Update(project);
            await _eventsPublisher.Publish(new CriticalStagePlanned(projectId, stageTimeSlot, resourceId,
                _timeProvider.GetUtcNow().DateTime));
        });
    }

    public async Task PlanCriticalStage(ProjectId projectId, Stage criticalStage, TimeSlot stageTimeSlot)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            project.AddSchedule(criticalStage, stageTimeSlot);
            await _projectRepository.Update(project);
            await _eventsPublisher.Publish(new CriticalStagePlanned(projectId, stageTimeSlot, null,
                _timeProvider.GetUtcNow().DateTime));
        });
    }

    public async Task DefineManualSchedule(ProjectId projectId, Schedule schedule)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            project.AddSchedule(schedule);
            await _projectRepository.Update(project);
        });
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