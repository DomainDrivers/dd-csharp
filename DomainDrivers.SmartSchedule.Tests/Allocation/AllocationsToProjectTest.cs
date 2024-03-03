using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Shared;
using NUnit.Framework.Legacy;
using static DomainDrivers.SmartSchedule.Shared.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class AllocationsToProjectTest
{
    static readonly DateTime When = DateTime.MinValue;
    static readonly ProjectAllocationsId ProjectId = ProjectAllocationsId.NewOne();
    static readonly AllocatableCapabilityId AdminId = AllocatableCapabilityId.NewOne();
    static readonly TimeSlot Feb1 = TimeSlot.CreateDailyTimeSlotAtUtc(2020, 2, 1);
    static readonly TimeSlot Feb2 = TimeSlot.CreateDailyTimeSlotAtUtc(2020, 2, 2);
    static readonly TimeSlot January = TimeSlot.CreateMonthlyTimeSlotAtUtc(2020, 1);
    static readonly TimeSlot February = TimeSlot.CreateMonthlyTimeSlotAtUtc(2020, 2);

    [Fact]
    public void CanAllocate()
    {
        //given
        var allocations = ProjectAllocations.Empty(ProjectId);

        //when
        var @event = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);

        //then
        Assert.NotNull(@event);
        Assert.Equal(
            new CapabilitiesAllocated(@event.EventId, @event.AllocatedCapabilityId,
                ProjectId, Demands.None(), When), @event);
    }

    [Fact]
    public void CantAllocateWhenRequestedTimeSlotNotWithinProjectSlot()
    {
        //given
        var allocations = new ProjectAllocations(ProjectId, Allocations.None(), Demands.None(), January);

        //when
        var @event = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);

        //then
        Assert.Null(@event);
    }

    [Fact]
    public void AllocatingHasNoEffectWhenCapabilityAlreadyAllocated()
    {
        //given
        var allocations = ProjectAllocations.Empty(ProjectId);

        //and
        allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);

        //when
        var @event = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);

        //then
        Assert.Null(@event);
    }

    [Fact]
    public void ThereAreNoMissingDemandsWhenAllAllocated()
    {
        //given
        var demands = Demands.Of(new Demand(Permission("ADMIN"), Feb1), new Demand(Skill("JAVA"), Feb1));
        //and
        var allocations = ProjectAllocations.WithDemands(ProjectId, demands);
        //and
        allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);
        //when
        var @event = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Skill("JAVA")), Feb1, When);
        //then
        Assert.NotNull(@event);
        Assert.Equal(
            new CapabilitiesAllocated(@event.EventId, @event.AllocatedCapabilityId, ProjectId, Demands.None(), When),
            @event);
    }

    [Fact]
    public void MissingDemandsArePresentWhenAllocatingForDifferentThanDemandedSlot()
    {
        //given
        var demands = Demands.Of(new Demand(Permission("ADMIN"), Feb1), new Demand(Skill("JAVA"), Feb1));
        //and
        var allocations = ProjectAllocations.WithDemands(ProjectId, demands);
        //and
        allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);
        //when
        var @event = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Skill("JAVA")), Feb2, When);
        //then
        Assert.NotNull(@event);
        Assert.Equal(Demands.Of(new Demand(Skill("JAVA"), Feb1)), allocations.MissingDemands());
        Assert.Equal(new CapabilitiesAllocated(@event.EventId, @event.AllocatedCapabilityId, ProjectId,
            Demands.Of(new Demand(Skill("JAVA"), Feb1)), When), @event);
    }

    [Fact]
    public void CanRelease()
    {
        //given
        var allocations = ProjectAllocations.Empty(ProjectId);
        //and
        var allocatedAdmin = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);
        //when
        var adminId = new AllocatableCapabilityId(allocatedAdmin!.AllocatedCapabilityId);
        var @event = allocations.Release(adminId, Feb1, When);

        //then
        Assert.NotNull(@event);
        Assert.Equal(@event, new CapabilityReleased(@event.EventId, ProjectId, Demands.None(), When));
    }

    [Fact]
    public void ReleasingHasNoEffectWhenCapabilityWasNotAllocated()
    {
        //given
        var allocations = ProjectAllocations.Empty(ProjectId);

        //when
        var @event = allocations.Release(AllocatableCapabilityId.NewOne(), Feb1, When);

        //then
        Assert.Null(@event);
    }

    [Fact]
    public void MissingDemandsArePresentAfterReleasingSomeOfAllocatedCapabilities()
    {
        //given
        var demandForJava = new Demand(Skill("JAVA"), Feb1);
        var demandForAdmin = new Demand(Permission("ADMIN"), Feb1);
        var allocations = ProjectAllocations.WithDemands(ProjectId, Demands.Of(demandForAdmin, demandForJava));
        //and
        var allocatedAdmin = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);
        allocations.Allocate(AllocatableCapabilityId.NewOne(),CapabilitySelector.CanJustPerform(Skill("JAVA")), Feb1, When);
        //when
        var @event = allocations.Release(new AllocatableCapabilityId(allocatedAdmin!.AllocatedCapabilityId), Feb1, When);

        //then
        Assert.NotNull(@event);
        Assert.Equal(@event, new CapabilityReleased(@event.EventId, ProjectId, Demands.Of(demandForAdmin), When));
    }

    [Fact]
    public void ReleasingHasNoEffectWhenReleasingSlotNotWithinAllocatedSlot()
    {
        //given
        var allocations = ProjectAllocations.Empty(ProjectId);
        //and
        var allocatedAdmin = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);

        //when
        var @event = allocations.Release(new AllocatableCapabilityId(allocatedAdmin!.AllocatedCapabilityId), Feb2, When);

        //then
        Assert.Null(@event);
    }

    [Fact]
    public void ReleasingSmallPartOfSlotLeavesTheRest()
    {
        //given
        var allocations = ProjectAllocations.Empty(ProjectId);
        //and
        var allocatedAdmin = allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);

        //when
        var fifteenMinutesIn1Feb = new TimeSlot(Feb1.From.AddHours(1), Feb1.From.AddHours(2));
        var oneHourBefore = new TimeSlot(Feb1.From, Feb1.From.AddHours(1));
        var theRest = new TimeSlot(Feb1.From.AddHours(2), Feb1.To);

        //when
        var @event = allocations.Release(new AllocatableCapabilityId(allocatedAdmin!.AllocatedCapabilityId), fifteenMinutesIn1Feb, When);

        //then
        Assert.Equal(new CapabilityReleased(@event!.EventId, ProjectId, Demands.None(), When), @event);
        CollectionAssert.AreEquivalent(new List<AllocatedCapability>
        {
            new AllocatedCapability(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), oneHourBefore),
            new AllocatedCapability(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), theRest)
        }, allocations.Allocations.All);
    }

    [Fact]
    public void CanChangeDemands()
    {
        //given
        var demands = Demands.Of(new Demand(Permission("ADMIN"), Feb1), new Demand(Skill("JAVA"), Feb1));
        //and
        var allocations = ProjectAllocations.WithDemands(ProjectId, demands);
        //and
        allocations.Allocate(AdminId, CapabilitySelector.CanJustPerform(Permission("ADMIN")), Feb1, When);
        //when
        var @event = allocations.AddDemands(Demands.Of(new Demand(Skill("PYTHON"), Feb1)), When);
        //then
        Assert.Equal(Demands.AllInSameTimeSlot(Feb1, Skill("JAVA"), Skill("PYTHON")),
            allocations.MissingDemands());
        Assert.Equal(new ProjectAllocationsDemandsScheduled(@event!.Uuid, ProjectId, Demands.AllInSameTimeSlot(
            Feb1, Skill("JAVA"),
            Skill("PYTHON")), When), @event);
    }

    [Fact]
    public void CanChangeProjectDates()
    {
        //given
        var allocations =
            new ProjectAllocations(ProjectId, Allocations.None(), Demands.None(), January);

        //when
        var @event = allocations.DefineSlot(February, When);

        //then
        Assert.NotNull(@event);
        Assert.Equal(new ProjectAllocationScheduled(@event.Uuid, ProjectId, February, When), @event);
    }
}