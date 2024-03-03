using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation;

public class PotentialTransfersService
{
    private readonly SimulationFacade _simulationFacade;
    private readonly CashFlowFacade _cashFlowFacade;
    private readonly IAllocationDbContext _allocationDbContext;

    public PotentialTransfersService(SimulationFacade simulationFacade, CashFlowFacade cashFlowFacade,
        IAllocationDbContext allocationDbContext)
    {
        _simulationFacade = simulationFacade;
        _cashFlowFacade = cashFlowFacade;
        _allocationDbContext = allocationDbContext;
    }

    public async Task<double> ProfitAfterMovingCapabilities(ProjectAllocationsId projectId,
        AllocatableCapabilitySummary capabilityToMove, TimeSlot timeSlot)
    {
        //cached?
        var potentialTransfers =
            new PotentialTransfers(
                ProjectsAllocationsSummary.Of(await _allocationDbContext.ProjectAllocations.ToListAsync()),
                await _cashFlowFacade.FindAllEarnings());
        return CheckPotentialTransfer(potentialTransfers, projectId, capabilityToMove, timeSlot);
    }

    private double CheckPotentialTransfer(PotentialTransfers transfers, ProjectAllocationsId projectTo,
        AllocatableCapabilitySummary capabilityToMove, TimeSlot forSlot)
    {
        var resultBefore =
            _simulationFacade.WhatIsTheOptimalSetup(transfers.ToSimulatedProjects(), SimulatedCapabilities.None());
        transfers = transfers.Transfer(projectTo, capabilityToMove, forSlot);
        var resultAfter =
            _simulationFacade.WhatIsTheOptimalSetup(transfers.ToSimulatedProjects(), SimulatedCapabilities.None());
        return resultAfter.Profit - resultBefore.Profit;
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