using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using MediatR;

namespace DomainDrivers.SmartSchedule.Risk;

public class RiskPeriodicCheckSagaDispatcher :
    INotificationHandler<EarningsRecalculated>, INotificationHandler<ProjectAllocationScheduled>,
    INotificationHandler<ResourceTakenOver>, INotificationHandler<NotSatisfiedDemands>
{
    private readonly RiskPeriodicCheckSagaRepository _riskSagaRepository;
    private readonly PotentialTransfersService _potentialTransfersService;
    private readonly CapabilityFinder _capabilityFinder;
    private readonly IRiskPushNotification _riskPushNotification;
    private readonly TimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public RiskPeriodicCheckSagaDispatcher(RiskPeriodicCheckSagaRepository riskSagaRepository,
        PotentialTransfersService potentialTransfersService, CapabilityFinder capabilityFinder,
        IRiskPushNotification riskPushNotification, TimeProvider clock, IUnitOfWork unitOfWork)
    {
        _riskSagaRepository = riskSagaRepository;
        _potentialTransfersService = potentialTransfersService;
        _capabilityFinder = capabilityFinder;
        _riskPushNotification = riskPushNotification;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    //remember about transactions spanning saga and potential external system
    public async Task Handle(ProjectAllocationScheduled @event, CancellationToken cancellationToken)
    {
        var (found, nextStep) = await _unitOfWork.InTransaction(async () =>
        {
            var found = await _riskSagaRepository.FindByProjectIdOrCreate(@event.ProjectAllocationsId);
            var nextStep = found.Handle(@event);
            return (found, nextStep);
        });
        await Perform(nextStep, found);
    }
    
    //remember about transactions spanning saga and potential external system
    public async Task Handle(NotSatisfiedDemands @event, CancellationToken cancellationToken)
    {
        var nextSteps = await _unitOfWork.InTransaction(async () =>
        {
            var sagas = await _riskSagaRepository.FindByProjectIdInOrElseCreate(
                new List<ProjectAllocationsId>(@event.MissingDemands.Keys));
            IDictionary<RiskPeriodicCheckSaga, RiskPeriodicCheckSagaStep?> nextSteps =
                new Dictionary<RiskPeriodicCheckSaga, RiskPeriodicCheckSagaStep?>();
            foreach (var saga in sagas)
            {
                var missingDemands = @event.MissingDemands[saga.ProjectId];
                var nextStep = saga.HandleMissingDemands(missingDemands);
                nextSteps[saga] = nextStep;
            }

            return nextSteps;
        });

        foreach (var (saga, nextStep) in nextSteps)
        {
            await Perform(nextStep, saga);
        }
    }
    
    //remember about transactions spanning saga and potential external system
    public async Task Handle(EarningsRecalculated @event, CancellationToken cancellationToken)
    {
        var (found, nextStep) = await _unitOfWork.InTransaction(async () =>
        {
            var found = await _riskSagaRepository.FindByProjectId(@event.ProjectId);

            if (found == null)
            {
                found = new RiskPeriodicCheckSaga(@event.ProjectId, @event.Earnings);
                await _riskSagaRepository.Add(found);
            }

            var nextStep = found.Handle(@event);
            return (found, nextStep);
        });
        await Perform(nextStep, found);
    }
    
    //remember about transactions spanning saga and potential external system
    public async Task Handle(ResourceTakenOver @event, CancellationToken cancellationToken)
    {
        var interested = @event.PreviousOwners
            .Select(owner => new ProjectAllocationsId(owner.OwnerId!.Value))
            .ToList();

        var sagas = await _riskSagaRepository.FindByProjectIdIn(interested);

        //transaction per one saga
        foreach (var saga in sagas)
        {
            await Handle(saga, @event);
        }
    }

    private async Task Handle(RiskPeriodicCheckSaga saga, ResourceTakenOver @event)
    {
        var nextStep = await _unitOfWork.InTransaction(() =>
        {
            var nextStep = saga.Handle(@event);
            return Task.FromResult(nextStep);
        });
        await Perform(nextStep, saga);
    }

    public async Task HandleWeeklyCheck()
    {
        var sagas = await _riskSagaRepository.FindAll();

        foreach (var saga in sagas)
        {
            var nextStep = await _unitOfWork.InTransaction(() =>
            {
                var nextStep = saga.HandleWeeklyCheck(_clock.GetUtcNow().DateTime);
                return Task.FromResult(nextStep);
            });
            await Perform(nextStep, saga);
        }
    }

    private async Task Perform(RiskPeriodicCheckSagaStep? nextStep, RiskPeriodicCheckSaga saga)
    {
        switch (nextStep)
        {
            case RiskPeriodicCheckSagaStep.NotifyAboutDemandsSatisfied:
                _riskPushNotification.NotifyDemandsSatisfied(saga.ProjectId);
                break;
            case RiskPeriodicCheckSagaStep.FindAvailable:
                await HandleFindAvailableFor(saga);
                break;
            case RiskPeriodicCheckSagaStep.DoNothing:
                break;
            case RiskPeriodicCheckSagaStep.SuggestReplacement:
                await HandleSimulateRelocation(saga);
                break;
            case RiskPeriodicCheckSagaStep.NotifyAboutPossibleRisk:
                _riskPushNotification.NotifyAboutPossibleRisk(saga.ProjectId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(nextStep), nextStep, null);
        }
    }

    private async Task HandleFindAvailableFor(RiskPeriodicCheckSaga saga)
    {
        var replacements = await FindAvailableReplacementsFor(saga.MissingDemands);

        if (replacements.Values.SelectMany(x => x.All).Any())
        {
            _riskPushNotification.NotifyAboutAvailability(saga.ProjectId, replacements);
        }
    }

    private async Task HandleSimulateRelocation(RiskPeriodicCheckSaga saga)
    {
        var possibleReplacements = await FindPossibleReplacements(saga.MissingDemands);

        foreach (var (demand, replacements) in possibleReplacements)
        {
            foreach (var replacement in replacements.All)
            {
                var profitAfterMovingCapabilities =
                    await _potentialTransfersService.ProfitAfterMovingCapabilities(saga.ProjectId, replacement,
                        replacement.TimeSlot);
                if (profitAfterMovingCapabilities > 0)
                {
                    _riskPushNotification.NotifyProfitableRelocationFound(saga.ProjectId, replacement.Id);
                }
            }
        }
    }

    private async Task<IDictionary<Demand, AllocatableCapabilitiesSummary>> FindAvailableReplacementsFor(
        Demands demands)
    {
        var replacements = new Dictionary<Demand, AllocatableCapabilitiesSummary>();

        foreach (var demand in demands.All)
        {
            var allocatableCapabilitiesSummary =
                await _capabilityFinder.FindAvailableCapabilities(demand.Capability, demand.Slot);
            replacements.Add(demand, allocatableCapabilitiesSummary);
        }

        return replacements;
    }

    private async Task<IDictionary<Demand, AllocatableCapabilitiesSummary>> FindPossibleReplacements(Demands demands)
    {
        var replacements = new Dictionary<Demand, AllocatableCapabilitiesSummary>();

        foreach (var demand in demands.All)
        {
            var allocatableCapabilitiesSummary =
                await _capabilityFinder.FindCapabilities(demand.Capability, demand.Slot);
            replacements.Add(demand, allocatableCapabilitiesSummary);
        }

        return replacements;
    }
}