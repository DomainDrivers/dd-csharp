using DomainDrivers.SmartSchedule.Availability.Segment;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Availability.Segment.SegmentInMinutes;

namespace DomainDrivers.SmartSchedule.Availability;

public class AvailabilityFacade
{
    private readonly ResourceAvailabilityRepository _availabilityRepository;
    private readonly ResourceAvailabilityReadModel _availabilityReadModel;
    private readonly IEventsPublisher _eventsPublisher;
    private readonly TimeProvider _timeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AvailabilityFacade(ResourceAvailabilityRepository availabilityRepository,
        ResourceAvailabilityReadModel availabilityReadModel, IEventsPublisher eventsPublisher,
        TimeProvider timeProvider, IUnitOfWork unitOfWork)
    {
        _availabilityRepository = availabilityRepository;
        _availabilityReadModel = availabilityReadModel;
        _eventsPublisher = eventsPublisher;
        _timeProvider = timeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateResourceSlots(ResourceId resourceId, TimeSlot timeslot)
    {
        var groupedAvailability = ResourceGroupedAvailability.Of(resourceId, timeslot);
        await _availabilityRepository.SaveNew(groupedAvailability);
    }

    public async Task CreateResourceSlots(ResourceId resourceId, ResourceId parentId,
        TimeSlot timeslot)
    {
        var groupedAvailability = ResourceGroupedAvailability.Of(resourceId, timeslot, parentId);
        await _availabilityRepository.SaveNew(groupedAvailability);
    }

    public async Task<bool> Block(ResourceId resourceId, TimeSlot timeSlot, Owner requester)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var toBlock = await FindGrouped(resourceId, timeSlot);
            return await Block(requester, toBlock);
        });
    }

    private async Task<bool> Block(Owner requester, ResourceGroupedAvailability toBlock)
    {
        if (toBlock.HasNoSlots)
        {
            return false;
        }
        var result = toBlock.Block(requester);

        if (result)
        {
            return await _availabilityRepository.SaveCheckingVersion(toBlock);
        }

        return result;
    }

    public async Task<bool> Release(ResourceId resourceId, TimeSlot timeSlot, Owner requester)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var toRelease = await FindGrouped(resourceId, timeSlot);
            if (toRelease.HasNoSlots)
            {
                return false;
            }

            var result = toRelease.Release(requester);

            if (result)
            {
                return await _availabilityRepository.SaveCheckingVersion(toRelease);
            }

            return result;
        });
    }

    public async Task<bool> Disable(ResourceId resourceId, TimeSlot timeSlot, Owner requester)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var toDisable = await FindGrouped(resourceId, timeSlot);
            if (toDisable.HasNoSlots)
            {
                return false;
            }

            var previousOwners = toDisable.Owners();
            var result = toDisable.Disable(requester);

            if (result)
            {
                result = await _availabilityRepository.SaveCheckingVersion(toDisable);

                if (result)
                {
                    await _eventsPublisher.Publish(new ResourceTakenOver(resourceId, previousOwners, timeSlot,
                        _timeProvider.GetUtcNow().DateTime));
                }
            }

            return result;
        });
    }

    public async Task<ResourceId?> BlockRandomAvailable(ISet<ResourceId> resourceIds, TimeSlot within, Owner owner)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var normalized = Segments.NormalizeToSegmentBoundaries(within, DefaultSegment());
            var groupedAvailability =
                await _availabilityRepository.LoadAvailabilitiesOfRandomResourceWithin(resourceIds, normalized);

            if (await Block(owner, groupedAvailability))
            {
                return groupedAvailability.ResourceId;
            }
            else
            {
                return null;
            }
        });
    }

    public async Task<ResourceGroupedAvailability> FindGrouped(ResourceId resourceId, TimeSlot within)
    {
        var normalized = Segments.NormalizeToSegmentBoundaries(within, DefaultSegment());
        return new ResourceGroupedAvailability(await _availabilityRepository.LoadAllWithinSlot(resourceId, normalized));
    }
    
    public async Task<Calendar> LoadCalendar(ResourceId resourceId, TimeSlot within) {
        var normalized = Segments.NormalizeToSegmentBoundaries(within, DefaultSegment());
        return await _availabilityReadModel.Load(resourceId, normalized);
    }

    public async Task<Calendars> LoadCalendars(ISet<ResourceId> resources, TimeSlot within) {
        var normalized = Segments.NormalizeToSegmentBoundaries(within, DefaultSegment());
        return await _availabilityReadModel.LoadAll(resources, normalized);
    }

    public async Task<ResourceGroupedAvailability> Find(ResourceId resourceId, TimeSlot within)
    {
        var normalized = Segments.NormalizeToSegmentBoundaries(within, DefaultSegment());
        return new ResourceGroupedAvailability(await _availabilityRepository.LoadAllWithinSlot(resourceId, normalized));
    }

    public async Task<ResourceGroupedAvailability> FindByParentId(ResourceId parentId, TimeSlot within)
    {
        var normalized = Segments.NormalizeToSegmentBoundaries(within, DefaultSegment());
        return new ResourceGroupedAvailability(
            await _availabilityRepository.LoadAllByParentIdWithinSlot(parentId, normalized));
    }
}