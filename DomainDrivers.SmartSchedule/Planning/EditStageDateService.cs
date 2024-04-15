using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public class EditStageDateService
{
    private readonly AllocationFacade _allocationFacade;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditStageDateService(AllocationFacade allocationFacade, IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _allocationFacade = allocationFacade;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task EditStageDate(ProjectId projectId, Stage stage, TimeSlot timeSlot)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _projectRepository.GetById(projectId);
            var schedule = project.Schedule;
            //redefine schedule
            //for each stage in schedule
            //recreate allocation
            //reallocate chosen resources (or find equivalents)
            //start risk analysis
        });
    }
}