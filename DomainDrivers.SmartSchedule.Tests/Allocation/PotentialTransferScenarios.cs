using DomainDrivers.SmartSchedule.Allocation;
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
    private static readonly Demands DemandForJavaJustFor15MinInJan = new Demands(new List<Demand> { new Demand(Skill("JAVA-MID"), FifteenMinutesInJan) });
    private static readonly Demands DemandForJavaMidInJan = new Demands(new List<Demand> { new Demand(Skill("JAVA-MID"), Jan1) });
    private static readonly Demands DemandsForJavaAndPythonInJan = new Demands(new List<Demand> { new Demand(Skill("JAVA-MID"), Jan1), new Demand(Skill("PYTHON-MID"), Jan1) });

    private static readonly Guid BankingSoftId = Guid.NewGuid();
    private static readonly Guid InsuranceSoftId = Guid.NewGuid();
    private static readonly AllocatedCapability StaszekJavaMid = new AllocatedCapability(Guid.NewGuid(), Skill("JAVA-MID"), Jan1);

    private readonly AllocationFacade _simulationFacade = new AllocationFacade(new SimulationFacade(new OptimizationFacade()));

    [Fact]
    public void SimulatesMovingCapabilitiesToDifferentProject()
    {
        //given
        var bankingSoft = new Project(DemandForJavaMidInJan, 9);
        var insuranceSoft = new Project(DemandForJavaMidInJan, 90);
        var projects = new Projects(new Dictionary<Guid, Project> { { BankingSoftId, bankingSoft }, { InsuranceSoftId, insuranceSoft } });
        //and
        bankingSoft.Add(StaszekJavaMid);

        //when
        var result = _simulationFacade.CheckPotentialTransfer(projects, BankingSoftId, InsuranceSoftId, StaszekJavaMid, Jan1);

        //then
        Assert.Equal(81d, result);
    }

    [Fact]
    public void SimulatesMovingCapabilitiesToDifferentProjectJustForAWhile()
    {
        //given
        var bankingSoft = new Project(DemandForJavaMidInJan, 9);
        var insuranceSoft = new Project(DemandForJavaJustFor15MinInJan, 99);
        var projects = new Projects(new Dictionary<Guid, Project> { { BankingSoftId, bankingSoft }, { InsuranceSoftId, insuranceSoft } });
        //and
        bankingSoft.Add(StaszekJavaMid);

        //when
        var result = _simulationFacade.CheckPotentialTransfer(projects, BankingSoftId, InsuranceSoftId, StaszekJavaMid, FifteenMinutesInJan);

        //then
        Assert.Equal(90d, result);
    }

    [Fact]
    public void TheMoveGivesZeroProfitWhenThereAreStillMissingDemands()
    {
        //given
        var bankingSoft = new Project(DemandForJavaMidInJan, 9);
        var insuranceSoft = new Project(DemandsForJavaAndPythonInJan, 99);
        var projects = new Projects(new Dictionary<Guid, Project> { { BankingSoftId, bankingSoft }, { InsuranceSoftId, insuranceSoft } });
        //and
        bankingSoft.Add(StaszekJavaMid);

        //when
        var result = _simulationFacade.CheckPotentialTransfer(projects, BankingSoftId, InsuranceSoftId, StaszekJavaMid, Jan1);

        //then
        Assert.Equal(-9d, result);
    }
}