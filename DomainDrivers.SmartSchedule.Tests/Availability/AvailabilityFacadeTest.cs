using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Availability;

public class AvailabilityFacadeTest
{
    private readonly AvailabilityFacade _availabilityFacade;

    public AvailabilityFacadeTest()
    {
        _availabilityFacade = new AvailabilityFacade();
    }

    [Fact]
    public async Task CanCreateAvailabilitySlots()
    {
        //given
        var resourceId = ResourceAvailabilityId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);

        //when
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);

        //then
        //todo check that availability(ies) was/were created
    }

    [Fact]
    public async Task CanBlockAvailabilities()
    {
        //given
        var resourceId = ResourceAvailabilityId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);

        //when
        var result = await _availabilityFacade.Block(resourceId, oneDay, owner);

        //then
        Assert.True(result);
        //todo check that can't be taken
    }

    [Fact]
    public async Task CanDisableAvailabilities()
    {
        //given
        var resourceId = ResourceAvailabilityId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);

        //when
        var result = await _availabilityFacade.Disable(resourceId, oneDay, owner);

        //then
        Assert.True(result);
        //todo check that are disabled
    }

    [Fact]
    public async Task CantBlockEvenWhenJustSmallSegmentOfRequestedSlotIsBlocked()
    {
        //given
        var resourceId = ResourceAvailabilityId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);
        //and
        await _availabilityFacade.Block(resourceId, oneDay, owner);
        var fifteenMinutes = new TimeSlot(oneDay.From, oneDay.From.AddMinutes(15));

        //when
        var result = await _availabilityFacade.Block(resourceId, fifteenMinutes, Owner.NewOne());

        //then
        Assert.False(result);
        //todo check that nothing was changed
    }


    [Fact]
    public async Task CanReleaseAvailability()
    {
        //given
        var resourceId = ResourceAvailabilityId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var fifteenMinutes = new TimeSlot(oneDay.From, oneDay.From.AddMinutes(15));
        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, fifteenMinutes);
        //and
        await _availabilityFacade.Block(resourceId, fifteenMinutes, owner);

        //when
        var result = await _availabilityFacade.Release(resourceId, oneDay, owner);

        //then
        Assert.True(result);
        //todo check can be taken again
    }

    [Fact]
    public async Task CantReleaseEvenWhenJustPartOfSlotIsOwnedByTheRequester()
    {
        //given
        var resourceId = ResourceAvailabilityId.NewOne();
        var jan1 = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var jan2 = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 2);
        var jan1_2 = new TimeSlot(jan1.From, jan2.To);
        var jan1Owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, jan1_2);
        //and
        await _availabilityFacade.Block(resourceId, jan1, jan1Owner);
        //and
        var jan2Owner = Owner.NewOne();
        await _availabilityFacade.Block(resourceId, jan2, jan2Owner);

        //when
        var result = await _availabilityFacade.Release(resourceId, jan1_2, jan1Owner);

        //then
        Assert.False(result);
        //todo check still owned by jan1
    }
}