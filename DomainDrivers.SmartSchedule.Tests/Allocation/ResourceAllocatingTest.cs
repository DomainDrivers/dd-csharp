using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class ResourceAllocatingTest : IntegrationTestWithSharedApp
{
    private static readonly AllocatableResourceId ResourceId = AllocatableResourceId.NewOne();
    private readonly AllocationFacade _allocationFacade;
    private readonly IAvailabilityFacade _availabilityFacade;
    private readonly CapabilityScheduler _capabilityScheduler;

    public ResourceAllocatingTest(IntegrationTestApp testApp) : base(testApp)
    {
        _allocationFacade = Scope.ServiceProvider.GetRequiredService<AllocationFacade>();
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<IAvailabilityFacade>();
        _capabilityScheduler = Scope.ServiceProvider.GetRequiredService<CapabilityScheduler>();
    }

    [Fact]
    public async Task CanAllocateCapabilityToProject()
    {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var skillJava = Capability.Skill("JAVA");
        var demand = new Demand(skillJava, oneDay);
        //and
        var allocatableCapabilityId = await CreateAllocatableResource(oneDay, skillJava, ResourceId);
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.Of(demand));

        //when
        var result = await _allocationFacade.AllocateToProject(projectId, allocatableCapabilityId, oneDay);

        //then
        Assert.True(result.HasValue);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Equal(
            new HashSet<AllocatedCapability>() { new AllocatedCapability(allocatableCapabilityId, CapabilitySelector.CanJustPerform(skillJava), oneDay) },
            summary.ProjectAllocations[projectId].All);
        Assert.Equal(new List<Demand>() { demand }, summary.Demands[projectId].All);
        Assert.True(await AvailabilityWasBlocked(allocatableCapabilityId.ToAvailabilityResourceId(), oneDay, projectId));
    }

    [Fact]
    public async Task CantAllocateWhenResourceNotAvailable()
    {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var skillJava = Capability.Skill("JAVA");
        var demand = new Demand(skillJava, oneDay);
        //and
        var allocatableCapabilityId = await CreateAllocatableResource(oneDay, skillJava, ResourceId);
        //and
        await _availabilityFacade.Block(allocatableCapabilityId.ToAvailabilityResourceId(), oneDay, Owner.NewOne());
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.Of(demand));

        //when
        var result = await _allocationFacade.AllocateToProject(projectId, allocatableCapabilityId, oneDay);

        //then
        Assert.False(result.HasValue);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Empty(summary.ProjectAllocations[projectId].All);
    }

    [Fact]
    public async Task CantAllocateWhenCapabilityHasNotBeenScheduled()
    {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var skillJava = Capability.Skill("JAVA");
        var demand = new Demand(skillJava, oneDay);
        //and
        var notScheduledCapability = AllocatableCapabilityId.NewOne();
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.Of(demand));

        //when
        var result = await _allocationFacade.AllocateToProject(projectId, notScheduledCapability, oneDay);

        //then
        Assert.False(result.HasValue);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Empty(summary.ProjectAllocations[projectId].All);
    }

    [Fact]
    public async Task CanReleaseCapabilityFromProject()
    {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        //and
        var allocatableCapabilityId = await CreateAllocatableResource(oneDay, Capability.Skill("JAVA"), ResourceId);
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.None());
        //and
        await _allocationFacade.AllocateToProject(projectId, allocatableCapabilityId, oneDay);

        //when
        var result = await _allocationFacade.ReleaseFromProject(projectId, allocatableCapabilityId, oneDay);

        //then
        Assert.True(result);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Empty(summary.ProjectAllocations[projectId].All);
        Assert.True(await AvailabilityIsReleased(oneDay, allocatableCapabilityId, projectId));
    }

    private async Task<AllocatableCapabilityId> ScheduleCapabilities(AllocatableResourceId allocatableResourceId,
        CapabilitySelector capabilities, TimeSlot oneDay)
    {
        var allocatableCapabilityIds =
            await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(allocatableResourceId,
                new List<CapabilitySelector>() { capabilities }, oneDay);
        Assert.Single(allocatableCapabilityIds);
        return allocatableCapabilityIds[0];
    }

    private async Task<AllocatableCapabilityId> CreateAllocatableResource(TimeSlot period, Capability capability,
        AllocatableResourceId resourceId)
    {
        var capabilitySelector = CapabilitySelector.CanJustPerform(capability);
        return await ScheduleCapabilities(resourceId, capabilitySelector, period);
    }

    private async Task<bool> AvailabilityWasBlocked(ResourceId resource, TimeSlot period,
        ProjectAllocationsId projectId)
    {
        var calendars = await _availabilityFacade.LoadCalendars(new HashSet<ResourceId> { resource }, period);
        return calendars.CalendarsDictionary.Values.All(calendar =>
            calendar.TakenBy(Owner.Of(projectId.Id)).SequenceEqual(new List<TimeSlot> { period }));
    }

    private async Task<bool> AvailabilityIsReleased(TimeSlot oneDay, AllocatableCapabilityId allocatableCapabilityId,
        ProjectAllocationsId projectId)
    {
        return !await AvailabilityWasBlocked(allocatableCapabilityId.ToAvailabilityResourceId(), oneDay, projectId);
    }
}