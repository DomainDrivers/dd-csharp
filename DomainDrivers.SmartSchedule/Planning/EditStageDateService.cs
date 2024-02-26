using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Planning;

public class EditStageDateService
{
    private readonly AllocationFacade _allocationFacade;
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IUnitOfWork _unitOfWork;

    public EditStageDateService(AllocationFacade allocationFacade, IPlanningDbContext planningDbContext,
        IUnitOfWork unitOfWork)
    {
        _allocationFacade = allocationFacade;
        _planningDbContext = planningDbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task EditStageDate(ProjectId projectId, Stage stage, TimeSlot timeSlot)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            var schedule = project.Schedule;
            //redefine schedule
            //for each stage in schedule
            //recreate allocation
            //reallocate chosen resources (or find equivalents)
            //start risk analysis
        });
    }
}