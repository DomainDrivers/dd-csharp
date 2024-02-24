using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class CapabilityAllocatingTest : IntegrationTest
{
    private static readonly AllocatableResourceId ResourceId = AllocatableResourceId.NewOne();
    private readonly AllocationFacade _allocationFacade;
    private readonly AvailabilityFacade _availabilityFacade;
    private readonly CapabilityScheduler _capabilityScheduler;

    public CapabilityAllocatingTest(IntegrationTestApp testApp) : base(testApp)
    {
        _allocationFacade = Scope.ServiceProvider.GetRequiredService<AllocationFacade>();
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<AvailabilityFacade>();
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
        var result = await _allocationFacade.AllocateToProject(projectId, allocatableCapabilityId, skillJava, oneDay);

        //then
        Assert.NotNull(result);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Equal(
            new HashSet<AllocatedCapability>()
                { new AllocatedCapability(allocatableCapabilityId, skillJava, oneDay) },
            summary.ProjectAllocations[projectId].All
        );
        Assert.True(summary.Demands[projectId].All.SequenceEqual(new[] { demand }));
        Assert.True(await AvailabilityWasBlocked(allocatableCapabilityId.ToAvailabilityResourceId(), oneDay, projectId));
    }
    
    [Fact]
    public async Task CantAllocateWhenResourceNotAvailable() {
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
        var result = await _allocationFacade.AllocateToProject(projectId, allocatableCapabilityId, skillJava, oneDay);

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
        var chosenCapability = Capability.Skill("JAVA");
        await _allocationFacade.AllocateToProject(projectId, allocatableCapabilityId, chosenCapability, oneDay);

        //when
        var result = await _allocationFacade.ReleaseFromProject(projectId, allocatableCapabilityId, oneDay);

        //then
        Assert.True(result);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Empty(summary.ProjectAllocations[projectId].All);
    }

    private async Task<AllocatableCapabilityId> CreateAllocatableResource(TimeSlot period, Capability capability, AllocatableResourceId resourceId)
    {
        var capabilities = new List<CapabilitySelector>() { CapabilitySelector.CanJustPerform(capability) };
        var allocatableCapabilityIds =
            await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(resourceId, capabilities, period);
        return allocatableCapabilityIds[0];
    }

    private async Task<bool> AvailabilityWasBlocked(ResourceId resource, TimeSlot period,
        ProjectAllocationsId projectId)
    {
        var calendars = await _availabilityFacade.LoadCalendars(new HashSet<ResourceId>() { resource }, period);
        return calendars.CalendarsDictionary.Values.All(calendar =>
            calendar.TakenBy(Owner.Of(projectId.Id)).SequenceEqual(new List<TimeSlot>() { period }));
    }
}