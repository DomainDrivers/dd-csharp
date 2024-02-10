using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;

namespace DomainDrivers.SmartSchedule.Allocation;

public class PotentialTransfersService
{
    private readonly SimulationFacade _simulationFacade;

    public PotentialTransfersService(SimulationFacade simulationFacade)
    {
        _simulationFacade = simulationFacade;
    }

    public double CheckPotentialTransfer(PotentialTransfers transfers, ProjectAllocationsId projectFrom,
        ProjectAllocationsId projectTo, AllocatedCapability capability, TimeSlot forSlot)
    {
        var resultBefore =
            _simulationFacade.WhatIsTheOptimalSetup(transfers.ToSimulatedProjects(), SimulatedCapabilities.None());
        transfers = transfers.Transfer(projectFrom, projectTo, capability, forSlot);
        var resultAfter =
            _simulationFacade.WhatIsTheOptimalSetup(transfers.ToSimulatedProjects(), SimulatedCapabilities.None());
        return resultAfter.Profit - resultBefore.Profit;
    }
}