using System.Linq.Expressions;
using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using NSubstitute;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class DemandSchedulingTest
{
    static readonly Demand Java = new Demand(Capability.Skill("JAVA"), TimeSlot.CreateDailyTimeSlotAtUtc(2022, 2, 2));

    static readonly TimeSlot ProjectDates = new TimeSlot(DateTime.Parse("2021-01-01T00:00:00.00Z"),
        DateTime.Parse("2021-01-06T00:00:00.00Z"));

    private readonly AllocationFacade _allocationFacade;

    public DemandSchedulingTest()
    {
        _allocationFacade = new AllocationFacade(new InMemoryProjectAllocationsRepository(),
            Substitute.For<IAvailabilityFacade>(), Substitute.For<ICapabilityFinder>(),
            Substitute.For<IEventsPublisher>(), TimeProvider.System, new InMemoryUnitOfWork());
    }

    [Fact]
    public async Task CanScheduleProjectDemands()
    {
        //given
        var projectId = ProjectAllocationsId.NewOne();

        //when
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.Of(Java));

        //then
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.True(summary.ProjectAllocations.ContainsKey(projectId));
        Assert.Empty(summary.ProjectAllocations[projectId].All);
        Assert.Equal(Demands.Of(Java).All, summary.Demands[projectId].All);
    }

    private static Expression<Predicate<ProjectAllocationsDemandsScheduled>> IsProjectDemandsScheduledEvent(ProjectAllocationsId projectId, Demands demands)
    {
        return @event =>
            @event.Uuid != default
            && @event.ProjectAllocationsId == projectId
            && @event.MissingDemands == demands
            && @event.OccurredAt != default;
    }
}