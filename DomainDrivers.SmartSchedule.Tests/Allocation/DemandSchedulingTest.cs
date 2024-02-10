using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class DemandSchedulingTest : IntegrationTest
{
    static readonly Demand Java = new Demand(Capability.Skill("JAVA"), TimeSlot.CreateDailyTimeSlotAtUtc(2022, 2, 2));

    static readonly TimeSlot ProjectDates = new TimeSlot(DateTime.Parse("2021-01-01T00:00:00.00Z"),
        DateTime.Parse("2021-01-06T00:00:00.00Z"));

    private readonly AllocationFacade _allocationFacade;

    public DemandSchedulingTest(IntegrationTestApp testApp) : base(testApp)
    {
        _allocationFacade = Scope.ServiceProvider.GetRequiredService<AllocationFacade>();
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
}