using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using Npgsql;

namespace DomainDrivers.SmartSchedule.Tests.Availability;

public class ResourceAvailabilityUniquenessTest : IntegrationTest
{
    static readonly TimeSlot OneMonth = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);

    private readonly ResourceAvailabilityRepository _resourceAvailabilityRepository;

    public ResourceAvailabilityUniquenessTest(IntegrationTestApp testApp) : base(testApp)
    {
        _resourceAvailabilityRepository = Scope.ServiceProvider.GetRequiredService<ResourceAvailabilityRepository>();
    }
    
    [Fact]
    public async Task CantSaveTwoAvailabilitiesWithSameResourceIdAndSegment() {
        //given
        var resourceId = ResourceAvailabilityId.NewOne();
        var anotherResourceId = ResourceAvailabilityId.NewOne();
        var resourceAvailabilityId = ResourceAvailabilityId.NewOne();

        //when
        await _resourceAvailabilityRepository.SaveNew(new ResourceAvailability(resourceAvailabilityId, resourceId, OneMonth));

        //expect
        var exception = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await _resourceAvailabilityRepository.SaveNew(new ResourceAvailability(resourceAvailabilityId, anotherResourceId, OneMonth));
        });
        Assert.Contains("duplicate key", exception.Message, StringComparison.InvariantCultureIgnoreCase);
    }
}