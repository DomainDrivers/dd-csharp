using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Availability;

public class ResourceAvailabilityOptimisticLockingTest : IntegrationTest
{
    private static readonly TimeSlot OneMonth = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
    
    private readonly ResourceAvailabilityRepository _resourceAvailabilityRepository;

    public ResourceAvailabilityOptimisticLockingTest(IntegrationTestApp testApp) : base(testApp)
    {
        _resourceAvailabilityRepository = Scope.ServiceProvider.GetRequiredService<ResourceAvailabilityRepository>();
    }

    [Fact]
    public async Task UpdateBumpsVersion()
    {
        //given
        var resourceAvailabilityId = ResourceAvailabilityId.NewOne();
        var resourceId = ResourceAvailabilityId.NewOne();
        var resourceAvailability = new ResourceAvailability(resourceAvailabilityId, resourceId, OneMonth);
        await _resourceAvailabilityRepository.SaveNew(resourceAvailability);
        
        //when
        resourceAvailability = await _resourceAvailabilityRepository.LoadById(resourceAvailabilityId);
        resourceAvailability.Block(Owner.NewOne());
        await _resourceAvailabilityRepository.SaveCheckingVersion(resourceAvailability);
        
        //then
        Assert.Equal(1, (await _resourceAvailabilityRepository.LoadById(resourceAvailability.Id)).Version);
    }
    
    [Fact]
    public async Task CantUpdateConcurrently()
    {
        //given
        var resourceAvailabilityId = ResourceAvailabilityId.NewOne();
        var resourceId = ResourceAvailabilityId.NewOne();
        var resourceAvailability = new ResourceAvailability(resourceAvailabilityId, resourceId, OneMonth);
        await _resourceAvailabilityRepository.SaveNew(resourceAvailability);
        var results = new List<bool>();
        //when
        var executor = new SemaphoreSlim(5, 5);
        var tasks = new List<Task>();
        for (var i = 1; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await executor.WaitAsync();
                //need to spawn new scope(new connection) in order to avoid conflicts which will result in exceptions
                //https://github.com/npgsql/npgsql/issues/3514#issuecomment-756787766
                using var scope = Scope.ServiceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ResourceAvailabilityRepository>();
                try
                {
                    var loaded = await repo.LoadById(resourceAvailabilityId);
                    loaded.Block(Owner.NewOne());
                    results.Add(await repo.SaveCheckingVersion(loaded));
                }
                catch (Exception)
                {
                    // ignore
                }
                finally
                {
                    executor.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
        
        //then
        Assert.Contains(false, results);
        Assert.True((await _resourceAvailabilityRepository.LoadById(resourceAvailabilityId)).Version < 10);
    }
}