using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Availability;

public class ResourceAvailabilityTest
{
    ResourceAvailabilityId ResourceAvailabilityId = ResourceAvailabilityId.NewOne();
    Owner OwnerOne = Owner.NewOne();
    Owner OwnerTwo = Owner.NewOne();

    [Fact]
    public void CanBeBlockedWhenIsAvailable() {
        //given
        var resourceAvailability = ResourceAvailability();

        //when
        var result = resourceAvailability.Block(OwnerOne);

        //then
        Assert.True(result);
    }

    [Fact]
    public void CantBeBlockedWhenAlreadyBlockedBySomeoneElse() {
        //given
        var resourceAvailability = ResourceAvailability();
        //and
        resourceAvailability.Block(OwnerOne);

        //when
        var result = resourceAvailability.Block(OwnerTwo);

        //then
        Assert.False(result);
    }

    [Fact]
    public void CanBeReleasedOnlyByInitialOwner() {
        //given
        var resourceAvailability = ResourceAvailability();
        //and
        resourceAvailability.Block(OwnerOne);

        //when
        var result = resourceAvailability.Release(OwnerOne);

        //then
        Assert.True(result);
    }

    [Fact]
    public void CantBeReleaseBySomeoneElse() {
        //given
        var resourceAvailability = ResourceAvailability();
        //and
        resourceAvailability.Block(OwnerOne);

        //when
        var result = resourceAvailability.Release(OwnerTwo);

        //then
        Assert.False(result);
    }

    [Fact]
    public void CanBeBlockedBySomeoneElseAfterReleasing() {
        //given
        var resourceAvailability = ResourceAvailability();
        //and
        resourceAvailability.Block(OwnerOne);
        //and
        resourceAvailability.Release(OwnerOne);

        //when
        var result = resourceAvailability.Release(OwnerTwo);

        //then
        Assert.True(result);
    }

    [Fact]
    public void CanDisableWhenAvailable() {
        //given
        var resourceAvailability = ResourceAvailability();

        //and
        var result = resourceAvailability.Disable(OwnerOne);

        //then
        Assert.True(result);
        Assert.True(resourceAvailability.IsDisabled);
        Assert.True(resourceAvailability.IsDisabledBy(OwnerOne));
    }

    [Fact]
    public void CanDisableWhenBlocked() {
        //given
        var resourceAvailability = ResourceAvailability();

        //and
        var resultBlocking = resourceAvailability.Block(OwnerOne);

        //when
        var resultDisabling = resourceAvailability.Disable(OwnerTwo);

        //then
        Assert.True(resultBlocking);
        Assert.True(resultDisabling);
        Assert.True(resourceAvailability.IsDisabled);
        Assert.True(resourceAvailability.IsDisabledBy(OwnerTwo));
    }

    [Fact]
    public void CantBeBlockedWhileDisabled() {
        //given
        var resourceAvailability = ResourceAvailability();

        //and
        var resultDisabling = resourceAvailability.Disable(OwnerOne);

        //when
        var resultBlocking = resourceAvailability.Block(OwnerTwo);
        var resultBlockingBySameOwner = resourceAvailability.Block(OwnerOne);

        //then
        Assert.True(resultDisabling);
        Assert.False(resultBlocking);
        Assert.False(resultBlockingBySameOwner);
        Assert.True(resourceAvailability.IsDisabled);
        Assert.True(resourceAvailability.IsDisabledBy(OwnerOne));
    }

    [Fact]
    public void CanBeEnabledByInitialRequester() {
        //given
        var resourceAvailability = ResourceAvailability();

        //and
        resourceAvailability.Disable(OwnerOne);

        //and
        var result = resourceAvailability.Enable(OwnerOne);

        //then
        Assert.True(result);
        Assert.False(resourceAvailability.IsDisabled);
        Assert.False(resourceAvailability.IsDisabledBy(OwnerOne));
    }

    [Fact]
    public void CantBeEnabledByAnotherRequester() {
        //given
        var resourceAvailability = ResourceAvailability();

        //and
        resourceAvailability.Disable(OwnerOne);

        //and
        var result = resourceAvailability.Enable(OwnerTwo);

        //then
        Assert.False(result);
        Assert.True(resourceAvailability.IsDisabled);
        Assert.True(resourceAvailability.IsDisabledBy(OwnerOne));
    }

    [Fact]
    public void CanBeBlockedAgainAfterEnabling() {
        //given
        var resourceAvailability = ResourceAvailability();

        //and
        resourceAvailability.Disable(OwnerOne);

        //and
        resourceAvailability.Enable(OwnerOne);

        //when
        var result = resourceAvailability.Block(OwnerTwo);

        //then
        Assert.True(result);
    }

    private ResourceAvailability ResourceAvailability() {
        return new ResourceAvailability(ResourceAvailabilityId, ResourceAvailabilityId.NewOne(), TimeSlot.CreateDailyTimeSlotAtUtc(2000, 1, 1));
    }
}