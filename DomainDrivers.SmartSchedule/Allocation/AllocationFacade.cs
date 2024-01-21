using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;

namespace DomainDrivers.SmartSchedule.Allocation;

public class AllocationFacade
{
    private readonly SimulationFacade _simulationFacade;

    public AllocationFacade(SimulationFacade simulationFacade)
    {
        this._simulationFacade = simulationFacade;
    }

    public double CheckPotentialTransfer(Projects projects, Guid projectFrom, Guid projectTo,
        AllocatedCapability capability, TimeSlot forSlot)
    {
        //Project rather fetched from db.
        var resultBefore =
            _simulationFacade.WhatIsTheOptimalSetup(projects.ToSimulatedProjects(), SimulatedCapabilities.None());
        projects = projects.Transfer(projectFrom, projectTo, capability, forSlot);
        var resultAfter =
            _simulationFacade.WhatIsTheOptimalSetup(projects.ToSimulatedProjects(), SimulatedCapabilities.None());
        return resultAfter.Profit - resultBefore.Profit;
    }
}