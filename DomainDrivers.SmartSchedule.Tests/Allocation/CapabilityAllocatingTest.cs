using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Shared.CapabilitySelector;
using static DomainDrivers.SmartSchedule.Shared.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class CapabilityAllocatingTest : IntegrationTestWithSharedApp
{
    private static readonly AllocatableResourceId AllocatableResourceId = AllocatableResourceId.NewOne();
    private static readonly AllocatableResourceId AllocatableResourceId2 = AllocatableResourceId.NewOne();
    private static readonly AllocatableResourceId AllocatableResourceId3 = AllocatableResourceId.NewOne();
    private readonly AllocationFacade _allocationFacade;
    private readonly IAvailabilityFacade _availabilityFacade;
    private readonly CapabilityScheduler _capabilityScheduler;

    public CapabilityAllocatingTest(IntegrationTestApp testApp) : base(testApp)
    {
        _allocationFacade = Scope.ServiceProvider.GetRequiredService<AllocationFacade>();
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<IAvailabilityFacade>();
        _capabilityScheduler = Scope.ServiceProvider.GetRequiredService<CapabilityScheduler>();
    }

    [Fact]
    public async Task CanAllocateAnyCapabilityOfRequiredType()
    {
        //given
        var javaAndPython = CanPerformOneOf(Skills("JAVA11", "PYTHON"));
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        //and
        var allocatableResourceId1 = await ScheduleCapabilities(AllocatableResourceId, javaAndPython, oneDay);
        var allocatableResourceId2 = await ScheduleCapabilities(AllocatableResourceId2, javaAndPython, oneDay);
        var allocatableResourceId3 = await ScheduleCapabilities(AllocatableResourceId3, javaAndPython, oneDay);
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.None());

        //when
        var result = await _allocationFacade.AllocateCapabilityToProjectForPeriod(projectId, Skill("JAVA11"), oneDay);

        //then
        Assert.True(result);
        var allocatedCapabilities = await LoadProjectAllocations(projectId);
        Assert.Contains(allocatedCapabilities, id => id == allocatableResourceId1 || id == allocatableResourceId2 || id == allocatableResourceId3);
        Assert.True(await AvailabilityWasBlocked(allocatedCapabilities, oneDay, projectId));
    }
    
    [Fact]
    public async Task CantAllocateAnyCapabilityOfRequiredTypeWhenNoCapabilities() {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.None());

        //when
        var result = await _allocationFacade.AllocateCapabilityToProjectForPeriod(projectId, Skill("DEBUGGING"), oneDay);

        //then
        Assert.False(result);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Empty(summary.ProjectAllocations[projectId].All);
    }

    [Fact]
    public async Task CantAllocateAnyCapabilityOfRequiredTypeWhenAllCapabilitiesTaken()
    {
        //given
        var capability = CanPerformOneOf(Skills("DEBUGGING"));
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);

        var allocatableCapabilityId1 = await ScheduleCapabilities(AllocatableResourceId, capability, oneDay);
        var allocatableCapabilityId2 = await ScheduleCapabilities(AllocatableResourceId2, capability, oneDay);
        //and
        var project1 =
            await _allocationFacade.CreateAllocation(oneDay, Demands.Of(new Demand(Skill("DEBUGGING"), oneDay)));
        var project2 =
            await _allocationFacade.CreateAllocation(oneDay, Demands.Of(new Demand(Skill("DEBUGGING"), oneDay)));
        //and
        await _allocationFacade.AllocateToProject(project1, allocatableCapabilityId1, oneDay);
        await _allocationFacade.AllocateToProject(project2, allocatableCapabilityId2, oneDay);

        //and
        var projectId = ProjectAllocationsId.NewOne();
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.None());

        //when
        var result = await _allocationFacade.AllocateCapabilityToProjectForPeriod(projectId, Skill("DEBUGGING"), oneDay);

        //then
        Assert.False(result);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Empty(summary.ProjectAllocations[projectId].All);
    }

    private async Task<ISet<AllocatableCapabilityId>> LoadProjectAllocations(ProjectAllocationsId projectId1)
    {
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        var allocatedCapabilities =
            summary
                .ProjectAllocations[projectId1]
                .All
                .Select(x => x.AllocatedCapabilityId)
                .ToHashSet();
        return allocatedCapabilities;
    }

    private async Task<AllocatableCapabilityId> ScheduleCapabilities(AllocatableResourceId allocatableResourceId, CapabilitySelector capabilities, TimeSlot period)
    {
        var allocatableCapabilityIds =
            await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(allocatableResourceId,
                new List<CapabilitySelector>() { capabilities }, period);
        return allocatableCapabilityIds[0];
    }

    private async Task<bool> AvailabilityWasBlocked(ISet<AllocatableCapabilityId> capabilities, TimeSlot oneDay,
        ProjectAllocationsId projectId)
    {
        var calendars =
            await _availabilityFacade.LoadCalendars(capabilities.Select(x => x.ToAvailabilityResourceId()).ToHashSet(),
                oneDay);
        return calendars.CalendarsDictionary.Values.All(calendar =>
            calendar.TakenBy(Owner.Of(projectId.Id)).SequenceEqual(new List<TimeSlot>() { oneDay }));
    }
}