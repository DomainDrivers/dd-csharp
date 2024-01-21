namespace DomainDrivers.SmartSchedule.Optimization;

public class OptimizationFacade
{
    public Result Calculate(IList<Item> items, TotalCapacity totalCapacity)
    {
        var capacitiesSize = totalCapacity.Size;
        var dp = new double[capacitiesSize + 1];
        var chosenItemsList = new List<Item>[capacitiesSize + 1];
        var allocatedCapacitiesList = new List<ISet<ICapacityDimension>>(capacitiesSize + 1);

        var automaticallyIncludedItems = items
            .Where(item => item.IsWeightZero)
            .ToList();
        var guaranteedValue = automaticallyIncludedItems
            .Sum(item => item.Value);

        for (var i = 0; i <= capacitiesSize; i++)
        {
            chosenItemsList[i] = new List<Item>();
            allocatedCapacitiesList.Add(new HashSet<ICapacityDimension>());
        }

        var allCapacities = totalCapacity.Capacities();
        var itemToCapacitiesMap =
            new Dictionary<Item, ISet<ICapacityDimension>>();

        foreach (var item in items.OrderByDescending(item => item.Value).ToList())
        {
            var chosenCapacities = MatchCapacities(item.TotalWeight, allCapacities);
            allCapacities = allCapacities.Except(chosenCapacities).ToList();

            if (chosenCapacities.Count == 0)
            {
                continue;
            }

            var sumValue = item.Value;
            var chosenCapacitiesCount = chosenCapacities.Count;

            for (var j = capacitiesSize; j >= chosenCapacitiesCount; j--)
            {
                if (dp[j] < sumValue + dp[j - chosenCapacitiesCount])
                {
                    dp[j] = sumValue + dp[j - chosenCapacitiesCount];

                    chosenItemsList[j] =
                        new List<Item>(chosenItemsList[j - chosenCapacitiesCount])
                        {
                            item
                        };

                    allocatedCapacitiesList[j].UnionWith(chosenCapacities);
                }
            }

            itemToCapacitiesMap.Add(item, new HashSet<ICapacityDimension>(chosenCapacities));
        }

        chosenItemsList[capacitiesSize].AddRange(automaticallyIncludedItems);
        return new Result(dp[capacitiesSize] + guaranteedValue,
            chosenItemsList[capacitiesSize],
            itemToCapacitiesMap);
    }

    private IList<ICapacityDimension> MatchCapacities(
        TotalWeight totalWeight,
        IList<ICapacityDimension> availableCapacities)
    {
        var result = new List<ICapacityDimension>();
        foreach (var weightComponent in totalWeight.Components())
        {
            var matchingCapacity = availableCapacities
                .FirstOrDefault(dimension => weightComponent.IsSatisfiedBy(dimension));

            if (matchingCapacity != null)
            {
                result.Add(matchingCapacity);
            }
            else
            {
                return new List<ICapacityDimension>();
            }
        }

        return result;
    }
}