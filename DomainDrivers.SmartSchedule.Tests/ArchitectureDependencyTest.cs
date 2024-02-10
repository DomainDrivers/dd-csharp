using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Assembly = System.Reflection.Assembly;

namespace DomainDrivers.SmartSchedule.Tests;

using static ArchRuleDefinition;

public class ArchitectureDependencyTest
{
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
        Assembly.Load("DomainDrivers.SmartSchedule")
    ).Build();

    private static readonly IObjectProvider<IType> AvailabilityLayer =
        Types().That().ResideInNamespace("DomainDrivers.SmartSchedule.Availability").As("Availability");

    private static readonly IObjectProvider<IType> SorterLayer =
        Types().That().ResideInNamespace("DomainDrivers.SmartSchedule.Sorter").As("Sorter");

    private static readonly IObjectProvider<IType> ParallelizationLayer =
        Types().That().ResideInNamespace("DomainDrivers.SmartSchedule.Planning.Parallelization").As("Parallelization");

    private static readonly IObjectProvider<IType> SharedLayer =
        Types().That().ResideInNamespace("DomainDrivers.SmartSchedule.Shared").As("Shared");

    private static readonly IObjectProvider<IType> SimulationLayer =
        Types().That().ResideInNamespace("DomainDrivers.SmartSchedule.Simulation").As("Simulation");

    private static readonly IObjectProvider<IType> OptimizationLayer =
        Types().That().ResideInNamespace("DomainDrivers.SmartSchedule.Optimization").As("Optimization");

    private static readonly IObjectProvider<IType> AllocationLayer =
        Types().That().ResideInNamespace("DomainDrivers.SmartSchedule.Allocation").As("Allocation");
    
    private static readonly IObjectProvider<IType> CashflowLayer =
        Types().That().ResideInNamespace("DomainDrivers.SmartSchedule.Allocation.Cashflow").As("Cashflow");

    [Fact]
    public void CheckDependencies()
    {
        Types().That().Are(AvailabilityLayer)
            .Should().NotDependOnAny(
                Types().That().AreNot(AvailabilityLayer)
                    .And().AreNot(SharedLayer))
            .Check(Architecture);
        Types().That().Are(ParallelizationLayer)
            .Should().NotDependOnAny(
                Types().That().AreNot(ParallelizationLayer)
                    .And().AreNot(SharedLayer)
                    .And().AreNot(SorterLayer))
            .Check(Architecture);
        Types().That().Are(SorterLayer)
            .Should().NotDependOnAny(
                Types().That().AreNot(SorterLayer)
                    .And().AreNot(SharedLayer))
            .Check(Architecture);
        Types().That().Are(SimulationLayer)
            .Should().NotDependOnAny(
                Types().That().AreNot(SimulationLayer)
                    .And().AreNot(OptimizationLayer)
                    .And().AreNot(SharedLayer))
            .Check(Architecture);
        Types().That().Are(OptimizationLayer)
            .Should().NotDependOnAny(
                Types().That().AreNot(OptimizationLayer)
                    .And().AreNot(SharedLayer))
            .Check(Architecture);
        Types().That().Are(SharedLayer)
            .Should().NotDependOnAny(
                Types().That().AreNot(SharedLayer))
            .Check(Architecture);
        Types().That().Are(AllocationLayer)
            .Should().NotDependOnAny(
                Types().That().AreNot(AllocationLayer)
                    .And().AreNot(SharedLayer)
                    .And().AreNot(AvailabilityLayer)
                    .And().AreNot(CashflowLayer)
                    .And().AreNot(SimulationLayer)
                    .And().AreNot(OptimizationLayer))
            .Check(Architecture);
    }
}