using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class CapabilityAllocatingTest : IntegrationTest
{
    private readonly AllocationFacade _allocationFacade;

    public CapabilityAllocatingTest(IntegrationTestApp testApp) : base(testApp)
    {
        _allocationFacade = Scope.ServiceProvider.GetRequiredService<AllocationFacade>();
    }

    [Fact]
    public async Task CanAllocateCapabilityToProject()
    {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var skillJava = Capability.Skill("JAVA");
        var demand = new Demand(skillJava, oneDay);
        //and
        var allocatableResourceId = ResourceId.NewOne();
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
            new HashSet<AllocatedCapability>() { new AllocatedCapability(allocatableResourceId.Id, skillJava, oneDay) },
            summary.ProjectAllocations[projectId].All
        );
        Assert.True(summary.Demands[projectId].All.SequenceEqual(new[] { demand }));
    }

    [Fact]
    public async Task CanReleaseCapabilityFromProject()
    {
        //given
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        //and
        var allocatableResourceId = ResourceId.NewOne();
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
}