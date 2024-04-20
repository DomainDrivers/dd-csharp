namespace DomainDrivers.SmartSchedule.Simulation;

public class SimulationFacade
{
    public Result WhichProjectWithMissingDemandsIsMostProfitableToAllocateResourcesTo(
        IList<SimulatedProject> projectsSimulations, SimulatedCapabilities totalCapability)
    {
        var list = totalCapability.Capabilities;
        var capacitiesSize = list.Count;
        var dp = new double[capacitiesSize + 1];
        var chosenItemsList = new List<SimulatedProject>[capacitiesSize + 1];
        var allocatedCapacitiesList = new List<ISet<AvailableResourceCapability>>(capacitiesSize + 1);

        var automaticallyIncludedItems = projectsSimulations
            .Where(project => project.AllDemandsSatisfied)
            .ToList();
        var guaranteedValue = automaticallyIncludedItems
            .Sum(project => decimal.ToDouble(project.Earnings));

        for (int i = 0; i <= capacitiesSize; i++)
        {
            chosenItemsList[i] = new List<SimulatedProject>();
            allocatedCapacitiesList.Add(new HashSet<AvailableResourceCapability>());
        }

        var allAvailabilities = new List<AvailableResourceCapability>(list);
        var itemToCapacitiesMap = new Dictionary<SimulatedProject, ISet<AvailableResourceCapability>>();

        foreach (var project in projectsSimulations.OrderByDescending(project => project.Earnings))
        {
            var chosenCapacities =
                MatchCapacities(project.MissingDemands, allAvailabilities);
            allAvailabilities = allAvailabilities.Except(chosenCapacities).ToList();

            if (chosenCapacities.Count == 0)
            {
                continue;
            }

            var sumValue = decimal.ToDouble(project.Earnings);
            var chosenCapacitiesCount = chosenCapacities.Count;

            for (int j = capacitiesSize; j >= chosenCapacitiesCount; j--)
            {
                if (dp[j] < sumValue + dp[j - chosenCapacitiesCount])
                {
                    dp[j] = sumValue + dp[j - chosenCapacitiesCount];

                    chosenItemsList[j] = new List<SimulatedProject>(chosenItemsList[j - chosenCapacitiesCount])
                    {
                        project
                    };

                    allocatedCapacitiesList[j].UnionWith(chosenCapacities);
                }
            }

            itemToCapacitiesMap[project] = new HashSet<AvailableResourceCapability>(chosenCapacities);
        }

        chosenItemsList[capacitiesSize].AddRange(automaticallyIncludedItems);
        return new Result(dp[capacitiesSize] + guaranteedValue, chosenItemsList[capacitiesSize], itemToCapacitiesMap);
    }

    private IList<AvailableResourceCapability> MatchCapacities(Demands demands,
        IList<AvailableResourceCapability> availableCapacities)
    {
        var result = new List<AvailableResourceCapability>();
        foreach (var singleDemand in demands.All)
        {
            var matchingCapacity = availableCapacities
                .FirstOrDefault(capability => singleDemand.IsSatisfiedBy(capability));

            if (matchingCapacity != null)
            {
                result.Add(matchingCapacity);
            }
            else
            {
                return new List<AvailableResourceCapability>();
            }
        }

        return result;
    }
}