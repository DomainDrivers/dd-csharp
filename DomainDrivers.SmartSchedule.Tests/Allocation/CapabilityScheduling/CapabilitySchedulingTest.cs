using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Allocation.CapabilityScheduling;

public class CapabilitySchedulingTest : IntegrationTest
{
    private readonly CapabilityScheduler _capabilityScheduler;
    private readonly CapabilityFinder _capabilityFinder;
    private readonly AvailabilityFacade _availabilityFacade;

    public CapabilitySchedulingTest(IntegrationTestApp testApp) : base(testApp)
    {
        _capabilityScheduler = Scope.ServiceProvider.GetRequiredService<CapabilityScheduler>();
        _capabilityFinder = Scope.ServiceProvider.GetRequiredService<CapabilityFinder>();
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<AvailabilityFacade>();
    }

    [Fact]
    public async Task CanScheduleAllocatableCapabilities()
    {
        //given
        var javaSkill = CapabilitySelector.CanJustPerform(Capability.Skill("JAVA"));
        var rustSkill = CapabilitySelector.CanJustPerform(Capability.Skill("RUST"));
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);

        //when
        var allocatable =
            await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(AllocatableResourceId.NewOne(),
                new List<CapabilitySelector> { javaSkill, rustSkill }, oneDay);

        //then
        var loaded = await _capabilityFinder.FindById(allocatable);
        Assert.Equal(allocatable.Count, loaded.All.Count);

        foreach (var allocatableCapability in loaded.All)
        {
            Assert.True(await AvailabilitySlotsAreCreated(allocatableCapability, oneDay));
        }
    }

    private async Task<bool> AvailabilitySlotsAreCreated(AllocatableCapabilitySummary allocatableCapability,
        TimeSlot oneDay)
    {
        var calendar =
            await _availabilityFacade.LoadCalendar(allocatableCapability.Id.ToAvailabilityResourceId(), oneDay);
        return calendar.AvailableSlots().SequenceEqual(new List<TimeSlot> { oneDay });
    }

    [Fact]
    public async Task CapabilityIsFoundWhenCapabilityPresentInTimeSlot()
    {
        //given
        var fitnessClass = Capability.Permission("FITNESS-CLASS");
        var uniqueSkill = CapabilitySelector.CanJustPerform(fitnessClass);
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var anotherDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 2);
        //and
        await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(AllocatableResourceId.NewOne(),
            new List<CapabilitySelector> { uniqueSkill }, oneDay);

        //when
        var found = await _capabilityFinder.FindAvailableCapabilities(fitnessClass, oneDay);
        var notFound = await _capabilityFinder.FindAvailableCapabilities(fitnessClass, anotherDay);

        //then
        Assert.Single(found.All);
        Assert.Empty(notFound.All);
        Assert.Equal(found.All[0].Capabilities, uniqueSkill);
        Assert.Equal(found.All[0].TimeSlot, oneDay);
    }

    [Fact]
    public async Task CapabilityNotFoundWhenCapabilityNotPresent()
    {
        //given
        var admin = CapabilitySelector.CanJustPerform(Capability.Permission("ADMIN"));
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        //and
        await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(AllocatableResourceId.NewOne(),
            new List<CapabilitySelector> { admin }, oneDay);

        //when
        var rustSkill = Capability.Skill("RUST JUST FOR NINJAS");
        var rust = CapabilitySelector.CanJustPerform(rustSkill);
        var found = await _capabilityFinder.FindCapabilities(rustSkill, oneDay);

        //then
        Assert.Empty(found.All);
    }

    [Fact]
    public async Task CanScheduleMultipleCapabilitiesOfSameType()
    {
        //given
        var loading = Capability.Skill("LOADING_TRUCK");
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        //and
        var truck1 = AllocatableResourceId.NewOne();
        var truck2 = AllocatableResourceId.NewOne();
        var truck3 = AllocatableResourceId.NewOne();
        await _capabilityScheduler.ScheduleMultipleResourcesForPeriod(
            new HashSet<AllocatableResourceId> { truck1, truck2, truck3 }, loading, oneDay);

        //when
        var found = await _capabilityFinder.FindCapabilities(loading, oneDay);

        //then
        Assert.Equal(3, found.All.Count);
    }

    [Fact]
    public async Task CanFindCapabilityIgnoringAvailability()
    {
        //given
        var adminPermission = Capability.Permission("REALLY_UNIQUE_ADMIN");
        var admin = CapabilitySelector.CanJustPerform(adminPermission);
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(1111, 1, 1);
        var differentDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 2, 1);
        var hourWithinDay = new TimeSlot(oneDay.From, oneDay.From.AddSeconds(3600));
        var partiallyOverlappingDay = new TimeSlot(oneDay.From.AddSeconds(3600), oneDay.To.AddSeconds(3600));
        //and
        await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(AllocatableResourceId.NewOne(),
            new List<CapabilitySelector> { admin }, oneDay);

        //when
        var onTheExactDay = await _capabilityFinder.FindCapabilities(adminPermission, oneDay);
        var onDifferentDay = await _capabilityFinder.FindCapabilities(adminPermission, differentDay);
        var inSlotWithin = await _capabilityFinder.FindCapabilities(adminPermission, hourWithinDay);
        var inOverlappingSlot = await _capabilityFinder.FindCapabilities(adminPermission, partiallyOverlappingDay);

        //then
        Assert.Single(onTheExactDay.All);
        Assert.Single(inSlotWithin.All);
        Assert.Empty(onDifferentDay.All);
        Assert.Empty(inOverlappingSlot.All);
    }

    [Fact]
    public async Task FindingTakesIntoAccountSimulationsCapabilities()
    {
        //given
        var truckAssets = new HashSet<Capability>() { Capability.Asset("LOADING"), Capability.Asset("CARRYING") };
        var truckCapabilities = CapabilitySelector.CanPerformAllAtTheTime(truckAssets);
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(1111, 1, 1);
        //and
        var truckResourceId = AllocatableResourceId.NewOne();
        await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(truckResourceId,
            new List<CapabilitySelector>() { truckCapabilities }, oneDay);

        //when
        var canPerformBoth =
            await _capabilityScheduler.FindResourceCapabilities(truckResourceId, truckAssets, oneDay);
        var canPerformJustLoading =
            await _capabilityScheduler.FindResourceCapabilities(truckResourceId, Capability.Asset("CARRYING"), oneDay);
        var canPerformJustCarrying =
            await _capabilityScheduler.FindResourceCapabilities(truckResourceId, Capability.Asset("LOADING"), oneDay);
        var cantPerformJava =
            await _capabilityScheduler.FindResourceCapabilities(truckResourceId, Capability.Skill("JAVA"), oneDay);

        //then
        Assert.NotNull(canPerformBoth);
        Assert.NotNull(canPerformJustLoading);
        Assert.NotNull(canPerformJustCarrying);
        Assert.Null(cantPerformJava);
    }
}