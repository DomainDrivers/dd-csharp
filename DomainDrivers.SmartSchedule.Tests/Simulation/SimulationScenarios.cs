using DomainDrivers.SmartSchedule.Optimization;
using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;
using static DomainDrivers.SmartSchedule.Simulation.Demand;
using static DomainDrivers.SmartSchedule.Simulation.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Simulation;

public class SimulationScenarios
{
    private static readonly TimeSlot Jan1 = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
    private static readonly ProjectId Project1 = ProjectId.NewOne();
    private static readonly ProjectId Project2 = ProjectId.NewOne();
    private static readonly ProjectId Project3 = ProjectId.NewOne();
    private static readonly Guid Staszek = Guid.NewGuid();
    private static readonly Guid Leon = Guid.NewGuid();

    private readonly SimulationFacade _simulationFacade = new SimulationFacade(new OptimizationFacade());

    [Fact]
    public void PicksOptimalProjectBasedOnEarnings()
    {
        //given
        var simulatedProjects = SimulatedProjects()
            .WithProject(Project1)
            .ThatRequires(DemandFor(Skill("JAVA-MID"), Jan1))
            .ThatCanEarn(9)
            .WithProject(Project2)
            .ThatRequires(DemandFor(Skill("JAVA-MID"), Jan1))
            .ThatCanEarn(99)
            .WithProject(Project3)
            .ThatRequires(DemandFor(Skill("JAVA-MID"), Jan1))
            .ThatCanEarn(2)
            .Build();

        //and there are
        var simulatedAvailability = SimulatedCapabilities()
            .WithEmployee(Staszek)
            .ThatBrings(Skill("JAVA-MID"))
            .ThatIsAvailableAt(Jan1)
            .WithEmployee(Leon)
            .ThatBrings(Skill("JAVA-MID"))
            .ThatIsAvailableAt(Jan1)
            .Build();

        //when
        var result =
            _simulationFacade.WhichProjectWithMissingDemandsIsMostProfitableToAllocateResourcesTo(simulatedProjects,
                simulatedAvailability);

        //then
        Assert.Equal(108d, result.Profit);
        Assert.Equal(2, result.ChosenItems.Count);
    }

    [Fact]
    public void PicksAllWhenEnoughCapabilities()
    {
        //given
        var simulatedProjects = SimulatedProjects()
            .WithProject(Project1)
            .ThatRequires(DemandFor(Skill("JAVA-MID"), Jan1))
            .ThatCanEarn(99)
            .Build();

        //and there are
        var simulatedAvailability = SimulatedCapabilities()
            .WithEmployee(Staszek)
            .ThatBrings(Skill("JAVA-MID"))
            .ThatIsAvailableAt(Jan1)
            .WithEmployee(Leon)
            .ThatBrings(Skill("JAVA-MID"))
            .ThatIsAvailableAt(Jan1)
            .Build();

        //when
        var result =
            _simulationFacade.WhichProjectWithMissingDemandsIsMostProfitableToAllocateResourcesTo(simulatedProjects,
                simulatedAvailability);

        //then
        Assert.Equal(99d, result.Profit);
        Assert.Equal(1, result.ChosenItems.Count);
    }

    [Fact]
    public void CanSimulateHavingExtraResources()
    {
        //given
        var simulatedProjects = SimulatedProjects()
            .WithProject(Project1)
            .ThatRequires(DemandFor(Skill("YT DRAMA COMMENTS"), Jan1))
            .ThatCanEarn(9)
            .WithProject(Project2)
            .ThatRequires(DemandFor(Skill("YT DRAMA COMMENTS"), Jan1))
            .ThatCanEarn(99)
            .Build();

        //and there are
        var simulatedAvailability = SimulatedCapabilities()
            .WithEmployee(Staszek)
            .ThatBrings(Skill("YT DRAMA COMMENTS"))
            .ThatIsAvailableAt(Jan1)
            .Build();

        //and there are
        var extraCapability =
            new AvailableResourceCapability(Guid.NewGuid(), Skill("YT DRAMA COMMENTS"), Jan1);

        //when
        var resultWithoutExtraResource =
            _simulationFacade.WhichProjectWithMissingDemandsIsMostProfitableToAllocateResourcesTo(simulatedProjects,
                simulatedAvailability);
        var resultWithExtraResource =
            _simulationFacade.WhichProjectWithMissingDemandsIsMostProfitableToAllocateResourcesTo(simulatedProjects,
                simulatedAvailability.Add(extraCapability));

        //then
        Assert.Equal(99d, resultWithoutExtraResource.Profit);
        Assert.Equal(108d, resultWithExtraResource.Profit);
    }

    private static SimulatedProjectsBuilder SimulatedProjects()
    {
        return new SimulatedProjectsBuilder();
    }

    private static AvailableCapabilitiesBuilder SimulatedCapabilities()
    {
        return new AvailableCapabilitiesBuilder();
    }
}