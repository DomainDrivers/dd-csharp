using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using NUnit.Framework.Legacy;
using static DomainDrivers.SmartSchedule.Availability.Segment.Segments;

namespace DomainDrivers.SmartSchedule.Tests.Availability;

public class AvailabilityCalendarTest : IntegrationTestWithSharedApp
{
    private readonly IAvailabilityFacade _availabilityFacade;
    private readonly IEventsPublisher _eventsPublisher;

    public AvailabilityCalendarTest(IntegrationTestApp testApp) : base(testApp)
    {
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<IAvailabilityFacade>();
        _eventsPublisher = Scope.ServiceProvider.GetRequiredService<IEventsPublisher>();
    }

    [Fact]
    public async Task LoadsCalendarForEntireMonth()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var durationOfSevenSlots = TimeSpan.FromMinutes(7 * DefaultSegmentDurationInMinutes);
        var sevenSlots = TimeSlot.CreateTimeSlotAtUtcOfDuration(2021, 1, 1, durationOfSevenSlots);
        var minimumSlot = new TimeSlot(sevenSlots.From, sevenSlots.From.AddMinutes(DefaultSegmentDurationInMinutes));
        var owner = Owner.NewOne();
        //and
        await _availabilityFacade.CreateResourceSlots(resourceId, sevenSlots);

        //when
        await _availabilityFacade.Block(resourceId, minimumSlot, owner);

        //then
        var calendar = await _availabilityFacade.LoadCalendar(resourceId, sevenSlots);
        Assert.Equal(new[] { minimumSlot }, calendar.TakenBy(owner));
        CollectionAssert.AreEquivalent(sevenSlots.LeftoverAfterRemovingCommonWith(minimumSlot), calendar.AvailableSlots());
    }

    [Fact]
    public async Task LoadsCalendarForMultipleResources()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var resourceId2 = ResourceId.NewOne();
        var durationOfSevenSlots = TimeSpan.FromMinutes(7 * DefaultSegmentDurationInMinutes);
        var sevenSlots = TimeSlot.CreateTimeSlotAtUtcOfDuration(2021, 1, 1, durationOfSevenSlots);
        var minimumSlot = new TimeSlot(sevenSlots.From, sevenSlots.From.AddMinutes(DefaultSegmentDurationInMinutes));

        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, sevenSlots);
        await _availabilityFacade.CreateResourceSlots(resourceId2, sevenSlots);

        //when
        await _availabilityFacade.Block(resourceId, minimumSlot, owner);
        await _availabilityFacade.Block(resourceId2, minimumSlot, owner);

        //then
        var calendars =
            await _availabilityFacade.LoadCalendars(new HashSet<ResourceId>() { resourceId, resourceId2 }, sevenSlots);
        Assert.Equal(new[] { minimumSlot }, calendars.Get(resourceId).TakenBy(owner));
        Assert.Equal(new[] { minimumSlot }, calendars.Get(resourceId2).TakenBy(owner));
        CollectionAssert.AreEquivalent(sevenSlots.LeftoverAfterRemovingCommonWith(minimumSlot),
            calendars.Get(resourceId).AvailableSlots());
        CollectionAssert.AreEquivalent(sevenSlots.LeftoverAfterRemovingCommonWith(minimumSlot),
            calendars.Get(resourceId2).AvailableSlots());
    }
}