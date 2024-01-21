using DomainDrivers.SmartSchedule.Optimization;

namespace DomainDrivers.SmartSchedule.Tests.Optimization;

public class OptimizationTest
{
    private readonly OptimizationFacade facade = new OptimizationFacade();

    [Fact]
    public void NothingIsChosenWhenNoCapacities()
    {
        //given
        var items = new List<Item>
        {
            new Item("Item1", 100, TotalWeight.Of(new CapabilityWeightDimension("COMMON SENSE", "Skill"))),
            new Item("Item2", 100, TotalWeight.Of(new CapabilityWeightDimension("THINKING", "Skill")))
        };

        //when
        var result = facade.Calculate(items, TotalCapacity.Zero());

        //then
        Assert.Equal(0, result.Profit);
        Assert.Empty(result.ChosenItems);
    }

    [Fact]
    public void EverythingIsChosenWhenAllWeightsAreZero()
    {
        //given
        var items = new List<Item>
        {
            new Item("Item1", 200, TotalWeight.Zero()),
            new Item("Item2", 100, TotalWeight.Zero())
        };

        //when
        var result = facade.Calculate(items, TotalCapacity.Zero());

        //then
        Assert.Equal(300, result.Profit);
        Assert.Equal(2, result.ChosenItems.Count);
    }

    [Fact]
    public void IfEnoughCapacityAllItemsAreChosen()
    {
        //given
        var items = new List<Item>
        {
            new Item("Item1", 100, TotalWeight.Of(new CapabilityWeightDimension("WEB DEVELOPMENT", "Skill"))),
            new Item("Item2", 300, TotalWeight.Of(new CapabilityWeightDimension("WEB DEVELOPMENT", "Skill")))
        };
        var c1 = new CapabilityCapacityDimension("anna", "WEB DEVELOPMENT", "Skill");
        var c2 = new CapabilityCapacityDimension("zbyniu", "WEB DEVELOPMENT", "Skill");

        //when
        var result = facade.Calculate(items, TotalCapacity.Of(c1, c2));

        //then
        Assert.Equal(400, result.Profit);
        Assert.Equal(2, result.ChosenItems.Count);
    }

    [Fact]
    public void MostValuableItemsAreChosen()
    {
        //given
        var item1 = new Item("Item1", 100, TotalWeight.Of(new CapabilityWeightDimension("JAVA", "Skill")));
        var item2 = new Item("Item2", 500, TotalWeight.Of(new CapabilityWeightDimension("JAVA", "Skill")));
        var item3 = new Item("Item3", 300, TotalWeight.Of(new CapabilityWeightDimension("JAVA", "Skill")));
        var c1 = new CapabilityCapacityDimension("anna", "JAVA", "Skill");
        var c2 = new CapabilityCapacityDimension("zbyniu", "JAVA", "Skill");

        //when
        var result = facade.Calculate(new List<Item> { item1, item2, item3 }, TotalCapacity.Of(c1, c2));

        //then
        Assert.Equal(800, result.Profit);
        Assert.Equal(2, result.ChosenItems.Count);
        var item3Capacity = Assert.Single(result.ItemToCapacities[item3]);
        Assert.True((CapabilityCapacityDimension)item3Capacity == c1 || (CapabilityCapacityDimension)item3Capacity == c2);
        var item2Capacity = Assert.Single(result.ItemToCapacities[item2]);
        Assert.True((CapabilityCapacityDimension)item2Capacity == c1 || (CapabilityCapacityDimension)item2Capacity == c2);
        Assert.False(result.ItemToCapacities.ContainsKey(item1));
    }
}