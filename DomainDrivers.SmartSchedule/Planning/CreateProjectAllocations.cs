using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Planning;

public class CreateProjectAllocations
{
    private readonly AllocationFacade _allocationFacade;
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectAllocations(AllocationFacade allocationFacade, IPlanningDbContext planningDbContext,
        IUnitOfWork unitOfWork)
    {
        _allocationFacade = allocationFacade;
        _planningDbContext = planningDbContext;
        _unitOfWork = unitOfWork;
    }

    //can react to ScheduleCalculated event
    public async Task Create(ProjectId projectId)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            var schedule = project.Schedule;
            //for each stage in schedule
            //create allocation
            //allocate chosen resources (or find equivalents)
            //start risk analysis
        });
    }
}