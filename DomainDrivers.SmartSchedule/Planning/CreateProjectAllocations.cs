using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public class CreateProjectAllocations
{
    private readonly AllocationFacade _allocationFacade;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectAllocations(AllocationFacade allocationFacade, IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _allocationFacade = allocationFacade;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    //can react to ScheduleCalculated event
    public async Task Create(ProjectId projectId)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            var schedule = project.Schedule;
            //for each stage in schedule
            //create allocation
            //allocate chosen resources (or find equivalents)
            //start risk analysis
        });
    }
}