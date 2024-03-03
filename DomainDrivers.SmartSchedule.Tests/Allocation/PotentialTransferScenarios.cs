using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Optimization;
using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;
using Demand = DomainDrivers.SmartSchedule.Allocation.Demand;
using Demands = DomainDrivers.SmartSchedule.Allocation.Demands;
using static DomainDrivers.SmartSchedule.Shared.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class PotentialTransferScenarios
{
    private static readonly TimeSlot Jan1 = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
    private static readonly TimeSlot FifteenMinutesInJan = new TimeSlot(Jan1.From, Jan1.From.AddMinutes(15));

    private static readonly Demands DemandForJavaJustFor15MinInJan = new Demands(new List<Demand>
        { new Demand(Skill("JAVA-MID"), FifteenMinutesInJan) });

    private static readonly Demands DemandForJavaMidInJan =
        new Demands(new List<Demand> { new Demand(Skill("JAVA-MID"), Jan1) });

    private static readonly Demands DemandsForJavaAndPythonInJan = new Demands(new List<Demand>
        { new Demand(Skill("JAVA-MID"), Jan1), new Demand(Skill("PYTHON-MID"), Jan1) });

    private static readonly ProjectAllocationsId BankingSoftId = ProjectAllocationsId.NewOne();
    private static readonly ProjectAllocationsId InsuranceSoftId = ProjectAllocationsId.NewOne();

    private static readonly AllocatedCapability StaszekJavaMid =
        new AllocatedCapability(AllocatableCapabilityId.NewOne(), CapabilitySelector.CanJustPerform(Skill("JAVA-MID")), Jan1);

    private readonly PotentialTransfersService _potentialTransfers =
        new PotentialTransfersService(new SimulationFacade(new OptimizationFacade()), null!, null!);

    [Fact]
    public void SimulatesMovingCapabilitiesToDifferentProject()
    {
        //given
        var bankingSoft = new Project(BankingSoftId, DemandForJavaMidInJan, Earnings.Of(9));
        var insuranceSoft = new Project(InsuranceSoftId, DemandForJavaMidInJan, Earnings.Of(90));
        bankingSoft.Add(StaszekJavaMid);
        var projects = ToPotentialTransfers(bankingSoft, insuranceSoft);

        //when
        var result =
            _potentialTransfers.CheckPotentialTransfer(projects, BankingSoftId, InsuranceSoftId, StaszekJavaMid, Jan1);

        //then
        Assert.Equal(81d, result);
    }

    [Fact]
    public void SimulatesMovingCapabilitiesToDifferentProjectJustForAWhile()
    {
        //given
        var bankingSoft = new Project(BankingSoftId, DemandForJavaMidInJan, Earnings.Of(9));
        var insuranceSoft = new Project(InsuranceSoftId, DemandForJavaJustFor15MinInJan, Earnings.Of(99));
        bankingSoft.Add(StaszekJavaMid);
        var projects = ToPotentialTransfers(bankingSoft, insuranceSoft);

        //when
        var result = _potentialTransfers.CheckPotentialTransfer(projects, BankingSoftId, InsuranceSoftId,
            StaszekJavaMid,
            FifteenMinutesInJan);

        //then
        Assert.Equal(90d, result);
    }

    [Fact]
    public void TheMoveGivesZeroProfitWhenThereAreStillMissingDemands()
    {
        //given
        var bankingSoft = new Project(BankingSoftId, DemandForJavaMidInJan, Earnings.Of(9));
        var insuranceSoft = new Project(InsuranceSoftId, DemandsForJavaAndPythonInJan, Earnings.Of(99));
        bankingSoft.Add(StaszekJavaMid);
        var projects = ToPotentialTransfers(bankingSoft, insuranceSoft);

        //when
        var result =
            _potentialTransfers.CheckPotentialTransfer(projects, BankingSoftId, InsuranceSoftId, StaszekJavaMid, Jan1);

        //then
        Assert.Equal(-9d, result);
    }

    private PotentialTransfers ToPotentialTransfers(params Project[] projects)
    {
        var allocations = new Dictionary<ProjectAllocationsId, Allocations>();
        var demands = new Dictionary<ProjectAllocationsId, Demands>();
        var earnings = new Dictionary<ProjectAllocationsId, Earnings>();
        foreach (var project in projects)
        {
            allocations[project.Id] = project.Allocations;
            demands[project.Id] = project.Demands;
            earnings[project.Id] = project.Earnings;
        }

        return new PotentialTransfers(
            new ProjectsAllocationsSummary(new Dictionary<ProjectAllocationsId, TimeSlot>(), allocations, demands),
            earnings);
    }
    
    private class Project
    {
        public ProjectAllocationsId Id { get; }
        public Earnings Earnings { get; }
        public Demands Demands { get; }
        public Allocations Allocations { get; private set; }

        public Project(ProjectAllocationsId id, Demands demands, Earnings earnings)
        {
            Id = id;
            Demands = demands;
            Earnings = earnings;
            Allocations = Allocations.None();
        }

        public Allocations Add(AllocatedCapability allocatedCapability)
        {
            Allocations = Allocations.Add(allocatedCapability);
            return Allocations;
        }
    }
}