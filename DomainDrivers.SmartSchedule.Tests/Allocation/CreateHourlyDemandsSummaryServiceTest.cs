using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Shared;
using NUnit.Framework.Legacy;
using static DomainDrivers.SmartSchedule.Shared.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class CreateHourlyDemandsSummaryServiceTest
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly TimeSlot Jan = TimeSlot.CreateMonthlyTimeSlotAtUtc(2021, 1);
    private static readonly Demands Csharp = Demands.Of(new Demand(Skill("CSHARP"), Jan));
    private static readonly Demands Java = Demands.Of(new Demand(Skill("JAVA"), Jan));

    private readonly CreateHourlyDemandsSummaryService _service = new CreateHourlyDemandsSummaryService();

    [Fact]
    public void CreatesMissingDemandsSummaryForAllGivenProjects()
    {
        //given
        var csharpProjectId = ProjectAllocationsId.NewOne();
        var javaProjectId = ProjectAllocationsId.NewOne();
        var csharpProject = new ProjectAllocations(csharpProjectId, Allocations.None(), Csharp, Jan);
        var javaProject = new ProjectAllocations(javaProjectId, Allocations.None(), Java, Jan);

        //when
        var result = _service.Create(new List<ProjectAllocations>() { csharpProject, javaProject }, Now);

        //then
        Assert.Equal(Now, result.OccurredAt);
        var expectedMissingDemands = new Dictionary<ProjectAllocationsId, Demands>()
        {
            { javaProjectId, Java },
            { csharpProjectId, Csharp }
        };
        CollectionAssert.AreEquivalent(expectedMissingDemands, result.MissingDemands);
    }

    [Fact]
    public void TakesIntoAccountOnlyProjectsWithTimeSlot()
    {
        //given
        var withTimeSlotId = ProjectAllocationsId.NewOne();
        var withoutTimeSlotId = ProjectAllocationsId.NewOne();
        var withTimeSlot = new ProjectAllocations(withTimeSlotId, Allocations.None(), Csharp, Jan);
        var withoutTimeSlot = new ProjectAllocations(withoutTimeSlotId, Allocations.None(), Java);

        //when
        var result = _service.Create(new List<ProjectAllocations>() { withTimeSlot, withoutTimeSlot }, Now);

        //then
        Assert.Equal(Now, result.OccurredAt);
        var expectedMissingDemands = new Dictionary<ProjectAllocationsId, Demands>() { { withTimeSlotId, Csharp } };
        CollectionAssert.AreEquivalent(expectedMissingDemands, result.MissingDemands);
    }
}