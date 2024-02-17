using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Availability;

public class AvailabilityFacadeTest : IntegrationTest
{
    private readonly AvailabilityFacade _availabilityFacade;

    public AvailabilityFacadeTest(IntegrationTestApp testApp) : base(testApp)
    {
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<AvailabilityFacade>();
    }

    [Fact]
    public async Task CanCreateAvailabilitySlots()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);

        //when
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);

        //then
        Assert.Equal(96, (await _availabilityFacade.Find(resourceId, oneDay)).Size);
    }

    [Fact]
    public async Task CanCreateNewAvailabilitySlotsWithParentId()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var resourceId2 = ResourceId.NewOne();
        var parentId = ResourceId.NewOne();
        var differentParentId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);

        //when
        await _availabilityFacade.CreateResourceSlots(resourceId, parentId, oneDay);
        await _availabilityFacade.CreateResourceSlots(resourceId2, differentParentId, oneDay);

        //then
        Assert.Equal(96, (await _availabilityFacade.FindByParentId(parentId, oneDay)).Size);
        Assert.Equal(96, (await _availabilityFacade.FindByParentId(differentParentId, oneDay)).Size);
    }

    [Fact]
    public async Task CanBlockAvailabilities()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);

        //when
        var result = await _availabilityFacade.Block(resourceId, oneDay, owner);

        //then
        Assert.True(result);
        var resourceAvailabilities = await _availabilityFacade.Find(resourceId, oneDay);
        Assert.Equal(96, resourceAvailabilities.Size);
        Assert.True(resourceAvailabilities.BlockedEntirelyBy(owner));
    }

    [Fact]
    public async Task CanDisableAvailabilities()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);

        //when
        var result = await _availabilityFacade.Disable(resourceId, oneDay, owner);

        //then
        Assert.True(result);
        var resourceAvailabilities = await _availabilityFacade.Find(resourceId, oneDay);
        Assert.Equal(96, resourceAvailabilities.Size);
        Assert.True(resourceAvailabilities.IsDisabledEntirelyBy(owner));
    }

    [Fact]
    public async Task CantBlockEvenWhenJustSmallSegmentOfRequestedSlotIsBlocked()
    {
        //given
        var resourceId = ResourceId.NewOne();
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
        var resourceAvailability = await _availabilityFacade.Find(resourceId, oneDay);
        Assert.True(resourceAvailability.BlockedEntirelyBy(owner));
    }


    [Fact]
    public async Task CanReleaseAvailability()
    {
        //given
        var resourceId = ResourceId.NewOne();
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
        var resourceAvailability = await _availabilityFacade.Find(resourceId, oneDay);
        Assert.True(resourceAvailability.IsEntirelyAvailable);
    }

    [Fact]
    public async Task CantReleaseEvenWhenJustPartOfSlotIsOwnedByTheRequester()
    {
        //given
        var resourceId = ResourceId.NewOne();
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
        var resourceAvailability = await _availabilityFacade.Find(resourceId, jan1);
        Assert.True(resourceAvailability.BlockedEntirelyBy(jan1Owner));
    }
    
    [Fact]
    public async Task OneSegmentCanBeTakenBySomeoneElseAfterRealising()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var fifteenMinutes = new TimeSlot(oneDay.From, oneDay.From.AddMinutes(15));
        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);
        //and
        await _availabilityFacade.Block(resourceId, oneDay, owner);
        //and
        await _availabilityFacade.Release(resourceId, fifteenMinutes, owner);

        //when
        var newRequester = Owner.NewOne();
        var result = await _availabilityFacade.Block(resourceId, fifteenMinutes, newRequester);

        //then
        Assert.True(result);
        var resourceAvailability = await _availabilityFacade.Find(resourceId, oneDay);
        Assert.Equal(96, resourceAvailability.Size);
        Assert.Equal(95, resourceAvailability.FindBlockedBy(owner).Count);
        Assert.Equal(1, resourceAvailability.FindBlockedBy(newRequester).Count);
    }
}