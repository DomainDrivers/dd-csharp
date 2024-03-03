using System.Linq.Expressions;
using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Shared;
using NSubstitute;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class CreatingNewProjectTest : IntegrationTestWithSharedApp
{
    static TimeSlot Jan = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
    static TimeSlot Feb = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 2, 1);

    private readonly AllocationFacade _allocationFacade;
    private readonly IEventsPublisher _eventsPublisher;

    public CreatingNewProjectTest(IntegrationTestApp testApp) : base(testApp)
    {
        _allocationFacade = Scope.ServiceProvider.GetRequiredService<AllocationFacade>();
        _eventsPublisher = Scope.ServiceProvider.GetRequiredService<IEventsPublisher>();
    }

    [Fact]
    public async Task CanCreateNewProject()
    {
        //given
        var demand = new Demand(Capability.Skill("JAVA"), Jan);

        //when
        var demands = Demands.Of(demand);
        var newProject = await _allocationFacade.CreateAllocation(Jan, demands);

        //then
        var summary =
            await _allocationFacade.FindAllProjectsAllocations(new HashSet<ProjectAllocationsId>() { newProject });
        Assert.Equal(demands, summary.Demands[newProject]);
        Assert.Equal(Jan, summary.TimeSlots[newProject]);
        await _eventsPublisher
            .Received(1)
            .Publish(Arg.Is(IsProjectAllocationsScheduledEvent(newProject, Jan)));
    }

    [Fact]
    public async Task CanRedefineProjectDeadline()
    {
        //given
        var demand = new Demand(Capability.Skill("JAVA"), Jan);
        //and
        var demands = Demands.Of(demand);
        var newProject = await _allocationFacade.CreateAllocation(Jan, demands);

        //when
        await _allocationFacade.EditProjectDates(newProject, Feb);

        //then
        var summary =
            await _allocationFacade.FindAllProjectsAllocations(new HashSet<ProjectAllocationsId>() { newProject });
        Assert.Equal(Feb, summary.TimeSlots[newProject]);
        await _eventsPublisher
            .Received(1)
            .Publish(Arg.Is(IsProjectAllocationsScheduledEvent(newProject, Feb)));
    }

    private static Expression<Predicate<ProjectAllocationScheduled>> IsProjectAllocationsScheduledEvent(ProjectAllocationsId projectId, TimeSlot timeSlot)
    {
        return @event =>
            @event.Uuid != default
            && @event.ProjectAllocationsId == projectId
            && @event.FromTo == timeSlot
            && @event.OccurredAt != default;
    }
}