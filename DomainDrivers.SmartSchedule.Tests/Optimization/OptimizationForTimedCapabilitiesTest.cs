using DomainDrivers.SmartSchedule.Optimization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Optimization;

public class OptimizationForTimedCapabilitiesTest
{
    private readonly OptimizationFacade facade = new OptimizationFacade();

    [Fact]
    public void NothingIsChosenWhenNoCapacitiesInTimeSlot()
    {
        //given
        var june = TimeSlot.CreateMonthlyTimeSlotAtUtc(2020, 6);
        var october = TimeSlot.CreateMonthlyTimeSlotAtUtc(2020, 10);

        var items = new List<Item>
        {
            new Item("Item1", 100,
                TotalWeight.Of(new CapabilityTimedWeightDimension("COMMON SENSE", "Skill", june))),
            new Item("Item2", 100,
                TotalWeight.Of(new CapabilityTimedWeightDimension("THINKING", "Skill", june)))
        };

        //when
        var result = facade.Calculate(items, TotalCapacity.Of(
            new CapabilityTimedCapacityDimension("anna", "COMMON SENSE", "Skill", october)
        ));

        //then
        Assert.Equal(0, result.Profit);
        Assert.Empty(result.ChosenItems);
    }

    [Fact]
    public void MostProfitableItemIsChosen()
    {
        //given
        var june = TimeSlot.CreateMonthlyTimeSlotAtUtc(2020, 6);

        var items = new List<Item>
        {
            new Item("Item1", 200,
                TotalWeight.Of(new CapabilityTimedWeightDimension("COMMON SENSE", "Skill", june))),
            new Item("Item2", 100,
                TotalWeight.Of(new CapabilityTimedWeightDimension("THINKING", "Skill", june)))
        };

        //when
        var result = facade.Calculate(items, TotalCapacity.Of(
            new CapabilityTimedCapacityDimension("anna", "COMMON SENSE", "Skill", june)
        ));

        //then
        Assert.Equal(200, result.Profit);
        Assert.Single(result.ChosenItems);
    }
}