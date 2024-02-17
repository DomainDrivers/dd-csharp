using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Planning;

public class PlanningFacade
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly StageParallelization _parallelization;
    private readonly PlanChosenResources _planChosenResourcesService;
    private readonly IUnitOfWork _unitOfWork;

    public PlanningFacade(IPlanningDbContext planningDbContext, StageParallelization parallelization,
        PlanChosenResources resourcesPlanning, IUnitOfWork unitOfWork)
    {
        _planningDbContext = planningDbContext;
        _parallelization = parallelization;
        _planChosenResourcesService = resourcesPlanning;
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
            await _planningDbContext.Projects.AddAsync(project);
            return project.Id;
        });
    }

    public async Task DefineStartDate(ProjectId projectId, DateTime possibleStartDate)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            project.AddSchedule(possibleStartDate);
        });
    }

    public async Task DefineProjectStages(ProjectId projectId, params Stage[] stages)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            var parallelizedStages = _parallelization.Of(new HashSet<Stage>(stages));
            project.DefineStages(parallelizedStages);
        });
    }

    public async Task AddDemands(ProjectId projectId, Demands demands)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            project.AddDemands(demands);
        });
    }

    public async Task DefineDemandsPerStage(ProjectId projectId, DemandsPerStage demandsPerStage)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            project.AddDemandsPerStage(demandsPerStage);
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
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            project.AddSchedule(criticalStage, stageTimeSlot);
        });
    }

    public async Task PlanCriticalStage(ProjectId projectId, Stage criticalStage, TimeSlot stageTimeSlot)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            project.AddSchedule(criticalStage, stageTimeSlot);
        });
    }

    public async Task DefineManualSchedule(ProjectId projectId, Schedule schedule)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            project.AddSchedule(schedule);
        });
    }

    public TimeSpan DurationOf(params Stage[] stages)
    {
        return DurationCalculator.Calculate(stages.ToList());
    }

    public async Task<ProjectCard> Load(ProjectId projectId)
    {
        var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
        return ToSummary(project);
    }

    public IList<ProjectCard> LoadAll(HashSet<ProjectId> projectsIds)
    {
        return _planningDbContext
            .Projects
            .Where(x => projectsIds.Contains(x.Id))
            .AsEnumerable()
            .Select(ToSummary)
            .ToList();
    }

    private ProjectCard ToSummary(Project project)
    {
        return new ProjectCard(project.Id, project.Name, project.ParallelizedStages, project.AllDemands,
            project.Schedule, project.DemandsPerStage, project.ChosenResources);
    }
}