using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Optimization;

public record Result(double Profit, IList<Item> ChosenItems, IDictionary<Item, ISet<ICapacityDimension>> ItemToCapacities)
{
    public virtual bool Equals(Result? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Profit == other.Profit
               && ChosenItems.SequenceEqual(other.ChosenItems)
               && ItemToCapacities.DictionaryEqual(other.ItemToCapacities,
                   EqualityComparer<ISet<ICapacityDimension>>.Create((x, y) => x!.SetEquals(y!)));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Profit, ChosenItems.CalculateHashCode(), ItemToCapacities.CalculateHashCode());
    }

    public override string ToString()
    {
        return $"Result{{profit={Profit}, chosenItems={ChosenItems.ToCollectionString()}}}";
    }
}