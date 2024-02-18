using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class CapabilityAllocatingTest : IntegrationTest
{
    private readonly AllocationFacade _allocationFacade;
    private readonly AvailabilityFacade _availabilityFacade;

    public CapabilityAllocatingTest(IntegrationTestApp testApp) : base(testApp)
    {
        _allocationFacade = Scope.ServiceProvider.GetRequiredService<AllocationFacade>();
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<AvailabilityFacade>();
    }

    [Fact]
    public async Task CanAllocateCapabilityToProject()
    {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var skillJava = Capability.Skill("JAVA");
        var demand = new Demand(skillJava, oneDay);
        //and
        var allocatableResourceId = await CreateAllocatableResource(oneDay);
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.Of(demand));

        //when
        var result = await _allocationFacade.AllocateToProject(projectId, allocatableResourceId, skillJava, oneDay);

        //then
        Assert.NotNull(result);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Equal(
            new HashSet<AllocatedCapability>()
                { new AllocatedCapability(allocatableResourceId.Id!.Value, skillJava, oneDay) },
            summary.ProjectAllocations[projectId].All
        );
        Assert.True(summary.Demands[projectId].All.SequenceEqual(new[] { demand }));
        Assert.True(await AvailabilityWasBlocked(allocatableResourceId, oneDay, projectId));
    }
    
    [Fact]
    public async Task CantAllocateWhenResourceNotAvailable() {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var skillJava = Capability.Skill("JAVA");
        var demand = new Demand(skillJava, oneDay);
        //and
        var allocatableResourceId = await CreateAllocatableResource(oneDay);
        //and
        await _availabilityFacade.Block(allocatableResourceId, oneDay, Owner.NewOne());
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.Of(demand));

        //when
        var result = await _allocationFacade.AllocateToProject(projectId, allocatableResourceId, skillJava, oneDay);

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
        var allocatableResourceId = await CreateAllocatableResource(oneDay);
        //and
        var projectId = ProjectAllocationsId.NewOne();
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(projectId, Demands.None());
        //and
        var chosenCapability = Capability.Skill("JAVA");
        var allocatedId =
            await _allocationFacade.AllocateToProject(projectId, allocatableResourceId, chosenCapability, oneDay);

        //when
        var result = await _allocationFacade.ReleaseFromProject(projectId, allocatedId!.Value, oneDay);

        //then
        Assert.True(result);
        var summary = await _allocationFacade.FindAllProjectsAllocations();
        Assert.Empty(summary.ProjectAllocations[projectId].All);
    }

    private async Task<ResourceId> CreateAllocatableResource(TimeSlot period)
    {
        var resourceId = ResourceId.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, period);
        return resourceId;
    }

    private async Task<bool> AvailabilityWasBlocked(ResourceId resource, TimeSlot period,
        ProjectAllocationsId projectId)
    {
        var calendars = await _availabilityFacade.LoadCalendars(new HashSet<ResourceId>() { resource }, period);
        return calendars.CalendarsDictionary.Values.All(calendar =>
            calendar.TakenBy(Owner.Of(projectId.Id)).SequenceEqual(new List<TimeSlot>() { period }));
    }
}