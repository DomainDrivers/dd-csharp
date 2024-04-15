using System.Linq.Expressions;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using NSubstitute;
using static DomainDrivers.SmartSchedule.Availability.Segment.Segments;

namespace DomainDrivers.SmartSchedule.Tests.Availability;

public class AvailabilityFacadeTest : IntegrationTestWithSharedApp
{
    private readonly IAvailabilityFacade _availabilityFacade;
    private readonly IEventsPublisher _eventsPublisher;

    public AvailabilityFacadeTest(IntegrationTestApp testApp) : base(testApp)
    {
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<IAvailabilityFacade>();
        _eventsPublisher = Scope.ServiceProvider.GetRequiredService<IEventsPublisher>();
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
        var entireMonth = TimeSlot.CreateMonthlyTimeSlotAtUtc(2021, 1);
        var monthlyCalendar = await _availabilityFacade.LoadCalendar(resourceId, entireMonth);
        Assert.Equal(Calendar.WithAvailableSlots(resourceId, oneDay), monthlyCalendar);
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
        Assert.True((await _availabilityFacade.FindByParentId(parentId, oneDay)).IsEntirelyWithParentId(parentId));
        Assert.True((await _availabilityFacade.FindByParentId(differentParentId, oneDay)).IsEntirelyWithParentId(differentParentId));
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
        var entireMonth = TimeSlot.CreateMonthlyTimeSlotAtUtc(2021, 1);
        var monthlyCalendar = await _availabilityFacade.LoadCalendar(resourceId, entireMonth);
        Assert.Empty(monthlyCalendar.AvailableSlots());
        Assert.True(monthlyCalendar.TakenBy(owner).SequenceEqual(new[] { oneDay }));
    }
    
    [Fact]
    public async Task CantBlockWhenNoSlotsCreated()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var owner = Owner.NewOne();

        //when
        var result = await _availabilityFacade.Block(resourceId, oneDay, owner);

        //then
        Assert.False(result);
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
        Assert.True(resourceAvailabilities.IsDisabledEntirelyBy(owner));
    }
    
    [Fact]
    public async Task CantDisableWhenNoSlotsCreated()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var owner = Owner.NewOne();

        //when
        var result = await _availabilityFacade.Disable(resourceId, oneDay, owner);

        //then
        Assert.False(result);
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
    public async Task CantReleaseWhenNoSlotsCreated()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var owner = Owner.NewOne();

        //when
        var result = await _availabilityFacade.Release(resourceId, oneDay, owner);

        //then
        Assert.False(result);
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
        var durationOfSevenSlots = TimeSpan.FromMinutes(7 * DefaultSegmentDurationInMinutes);
        var sevenSlots = TimeSlot.CreateTimeSlotAtUtcOfDuration(2021, 1, 1, durationOfSevenSlots);
        var minimumSlot = new TimeSlot(sevenSlots.From, sevenSlots.From.AddMinutes(DefaultSegmentDurationInMinutes));
        var owner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, sevenSlots);
        //and
        await _availabilityFacade.Block(resourceId, sevenSlots, owner);
        //and
        await _availabilityFacade.Release(resourceId, minimumSlot, owner);

        //when
        var newRequester = Owner.NewOne();
        var result = await _availabilityFacade.Block(resourceId, minimumSlot, newRequester);

        //then
        Assert.True(result);
        var dailyCalendar = await _availabilityFacade.LoadCalendar(resourceId, sevenSlots);
        Assert.Empty(dailyCalendar.AvailableSlots());
        Assert.True(dailyCalendar.TakenBy(owner).SequenceEqual(sevenSlots.LeftoverAfterRemovingCommonWith(minimumSlot)));
        Assert.True(dailyCalendar.TakenBy(newRequester).SequenceEqual(new[] { minimumSlot }));
    }

    [Fact]
    public async Task ResourceTakenOverEventIsEmittedAfterTakingOverTheResource()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var initialOwner = Owner.NewOne();
        var newOwner = Owner.NewOne();
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);
        await _availabilityFacade.Block(resourceId, oneDay, initialOwner);

        //when
        var result = await _availabilityFacade.Disable(resourceId, oneDay, newOwner);

        //then
        Assert.True(result);
        await _eventsPublisher
            .Received(1)
            .Publish(Arg.Is(TakenOver(resourceId, initialOwner, oneDay)));
    }

    private static Expression<Predicate<ResourceTakenOver>> TakenOver(ResourceId resourceId, Owner initialOwner, TimeSlot oneDay)
    {
        return @event =>
            @event.ResourceId == resourceId
            && @event.Slot == oneDay
            && @event.PreviousOwners.SetEquals(new HashSet<Owner>() { initialOwner })
            && @event.OccurredAt != default
            && @event.EventId != default;
    }
}