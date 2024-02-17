using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using NUnit.Framework.Legacy;

namespace DomainDrivers.SmartSchedule.Tests.Availability;

public class AvailabilityCalendarTest : IntegrationTest
{
    private readonly AvailabilityFacade _availabilityFacade;

    public AvailabilityCalendarTest(IntegrationTestApp testApp) : base(testApp)
    {
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<AvailabilityFacade>();
    }

    [Fact]
    public async Task LoadsCalendarForEntireMonth()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var fifteenMinutes = new TimeSlot(oneDay.From.AddMinutes(15), oneDay.From.AddMinutes(30));
        var owner = Owner.NewOne();
        //and
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);

        //when
        await _availabilityFacade.Block(resourceId, fifteenMinutes, owner);

        //then
        var calendar = await _availabilityFacade.LoadCalendar(resourceId, oneDay);
        Assert.Equal(new[] { fifteenMinutes }, calendar.TakenBy(owner));
        CollectionAssert.AreEquivalent(oneDay.LeftoverAfterRemovingCommonWith(fifteenMinutes), calendar.AvailableSlots());
    }

    [Fact]
    public async Task LoadsCalendarForMultipleResources()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var resourceId2 = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var fifteenMinutes = new TimeSlot(oneDay.From.AddMinutes(15), oneDay.From.AddMinutes(30));

        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);
        await _availabilityFacade.CreateResourceSlots(resourceId2, oneDay);

        //when
        await _availabilityFacade.Block(resourceId, fifteenMinutes, owner);
        await _availabilityFacade.Block(resourceId2, fifteenMinutes, owner);

        //then
        var calendars =
            await _availabilityFacade.LoadCalendars(new HashSet<ResourceId>() { resourceId, resourceId2 }, oneDay);
        Assert.Equal(new[] { fifteenMinutes }, calendars.Get(resourceId).TakenBy(owner));
        Assert.Equal(new[] { fifteenMinutes }, calendars.Get(resourceId2).TakenBy(owner));
        CollectionAssert.AreEquivalent(oneDay.LeftoverAfterRemovingCommonWith(fifteenMinutes), calendars.Get(resourceId).AvailableSlots());
        CollectionAssert.AreEquivalent(oneDay.LeftoverAfterRemovingCommonWith(fifteenMinutes), calendars.Get(resourceId2).AvailableSlots());
    }
}