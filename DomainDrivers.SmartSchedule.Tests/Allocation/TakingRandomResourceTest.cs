using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class TakingRandomResourceTest : IntegrationTestWithSharedApp
{
    private readonly AvailabilityFacade _availabilityFacade;

    public TakingRandomResourceTest(IntegrationTestApp testApp) : base(testApp)
    {
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<AvailabilityFacade>();
    }

    [Fact]
    public async Task CanTakeRandomResourceFromPool()
    {
        //given
        var resourceId = ResourceId.NewOne();
        var resourceId2 = ResourceId.NewOne();
        var resourceId3 = ResourceId.NewOne();
        var resourcesPool = new HashSet<ResourceId> { resourceId, resourceId2, resourceId3 };
        //and
        var owner1 = Owner.NewOne();
        var owner2 = Owner.NewOne();
        var owner3 = Owner.NewOne();
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);

        //and
        await _availabilityFacade.CreateResourceSlots(resourceId, oneDay);
        await _availabilityFacade.CreateResourceSlots(resourceId2, oneDay);
        await _availabilityFacade.CreateResourceSlots(resourceId3, oneDay);

        //when
        var taken1 = await _availabilityFacade.BlockRandomAvailable(resourcesPool, oneDay, owner1);

        //then
        Assert.NotNull(taken1);
        Assert.Contains(taken1, resourcesPool);
        await AssertThatResourceIsTakeByOwner(taken1, owner1, oneDay);

        //when
        var taken2 = await _availabilityFacade.BlockRandomAvailable(resourcesPool, oneDay, owner2);

        //then
        Assert.NotNull(taken2);
        Assert.Contains(taken2, resourcesPool);
        await AssertThatResourceIsTakeByOwner(taken2, owner2, oneDay);

        //when
        var taken3 = await _availabilityFacade.BlockRandomAvailable(resourcesPool, oneDay, owner3);

        //then
        Assert.NotNull(taken3);
        Assert.Contains(taken3, resourcesPool);
        await AssertThatResourceIsTakeByOwner(taken3, owner3, oneDay);

        //when
        var taken4 = await _availabilityFacade.BlockRandomAvailable(resourcesPool, oneDay, owner3);

        //then
        Assert.Null(taken4);
    }

    [Fact]
    public async Task NothingIsTakenWhenNoResourceInPool()
    {
        //given
        var resources = new HashSet<ResourceId> { ResourceId.NewOne(), ResourceId.NewOne(), ResourceId.NewOne() };

        //when
        var jan1 = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var taken1 = await _availabilityFacade.BlockRandomAvailable(resources, jan1, Owner.NewOne());

        //then
        Assert.Null(taken1);
    }

    private async Task AssertThatResourceIsTakeByOwner(ResourceId resourceId, Owner owner, TimeSlot oneDay)
    {
        var resourceAvailability = await _availabilityFacade.Find(resourceId, oneDay);
        Assert.All(resourceAvailability.Availabilities, ra => Assert.Equal(owner, ra.BlockedBy));
    }
}