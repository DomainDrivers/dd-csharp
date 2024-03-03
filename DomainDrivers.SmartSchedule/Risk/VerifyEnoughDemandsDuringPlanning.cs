using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Resource;
using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;
using MediatR;
using Demand = DomainDrivers.SmartSchedule.Simulation.Demand;
using Demands = DomainDrivers.SmartSchedule.Simulation.Demands;
using ProjectId = DomainDrivers.SmartSchedule.Simulation.ProjectId;

namespace DomainDrivers.SmartSchedule.Risk;

public class VerifyEnoughDemandsDuringPlanning : INotificationHandler<CapabilitiesDemanded>
{
    private const int SameArbitraryValueForEveryProject = 100;

    private readonly PlanningFacade _planningFacade;
    private readonly SimulationFacade _simulationFacade;
    private readonly ResourceFacade _resourceFacade;
    private readonly IRiskPushNotification _riskPushNotification;

    public VerifyEnoughDemandsDuringPlanning(PlanningFacade planningFacade, SimulationFacade simulationFacade,
        ResourceFacade resourceFacade, IRiskPushNotification riskPushNotification)
    {
        _planningFacade = planningFacade;
        _simulationFacade = simulationFacade;
        _resourceFacade = resourceFacade;
        _riskPushNotification = riskPushNotification;
    }


    public async Task Handle(CapabilitiesDemanded capabilitiesDemanded, CancellationToken cancellationToken)
    {
        var projectSummaries = await _planningFacade.LoadAll();
        var allCapabilities = await _resourceFacade.FindAllCapabilities();

        if (NotAbleToHandleAllProjectsGivenCapabilities(projectSummaries, allCapabilities))
        {
            _riskPushNotification.NotifyAboutPossibleRiskDuringPlanning(capabilitiesDemanded.ProjectId,
                capabilitiesDemanded.Demands);
        }
    }

    private bool NotAbleToHandleAllProjectsGivenCapabilities(IList<ProjectCard> projectSummaries,
        IList<Capability> allCapabilities)
    {
        var capabilities = allCapabilities
            .Select(cap =>
                new AvailableResourceCapability(Guid.NewGuid(), CapabilitySelector.CanJustPerform(cap),
                    TimeSlot.Empty()))
            .ToList();
        var simulatedProjects = projectSummaries
            .Select(x => CreateSamePriceSimulatedProject(x))
            .ToList();
        var result =
            _simulationFacade.WhatIsTheOptimalSetup(simulatedProjects, new SimulatedCapabilities(capabilities));
        return result.ChosenItems.Count != projectSummaries.Count;
    }

    private SimulatedProject CreateSamePriceSimulatedProject(ProjectCard card)
    {
        var simulatedDemands =
            card.Demands.All.Select(demand => new Demand(demand.Capability, TimeSlot.Empty())).ToList();
        return new SimulatedProject(ProjectId.From(card.ProjectId.Id), () => SameArbitraryValueForEveryProject,
            new Demands(simulatedDemands));
    }
}