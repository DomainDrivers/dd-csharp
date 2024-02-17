using DomainDrivers.SmartSchedule.Availability.Segment;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Availability.Segment.SegmentInMinutes;

namespace DomainDrivers.SmartSchedule.Availability;

public class AvailabilityFacade
{
    private readonly ResourceAvailabilityRepository _availabilityRepository;
    private readonly ResourceAvailabilityReadModel _availabilityReadModel;
    private readonly IUnitOfWork _unitOfWork;

    public AvailabilityFacade(ResourceAvailabilityRepository availabilityRepository,
        ResourceAvailabilityReadModel availabilityReadModel, IUnitOfWork unitOfWork)
    {
        _availabilityRepository = availabilityRepository;
        _availabilityReadModel = availabilityReadModel;
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
            var result = toDisable.Disable(requester);

            if (result)
            {
                result = await _availabilityRepository.SaveCheckingVersion(toDisable);
            }

            return result;
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