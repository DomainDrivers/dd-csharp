using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework.Legacy;
using Testcontainers.Redis;
using static DomainDrivers.SmartSchedule.Shared.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public class RedisRepositoryTest : IntegrationTest, IAsyncLifetime, IClassFixture<IntegrationTestAppWithRedis>
{
    private readonly IntegrationTestAppWithRedis _testApp;

    private static readonly TimeSlot Jan10_20 = new TimeSlot(DateTime.Parse("2020-01-10T00:00:00.00Z"),
        DateTime.Parse("2020-01-20T00:00:00.00Z"));

    private static readonly ChosenResources NeededResources =
        new ChosenResources(new HashSet<ResourceId> { ResourceId.NewOne() }, Jan10_20);

    private static readonly Schedule Schedule = new Schedule(new Dictionary<string, TimeSlot>
        { { "Stage1", Jan10_20 } });

    private static readonly Demands DemandForJava = new Demands(new List<Demand> { new Demand(Skill("JAVA")) });
    private static readonly DemandsPerStage DemandsPerStage = DemandsPerStage.Empty();

    private static readonly ParallelStagesList Stages =
        ParallelStagesList.Of(new ParallelStages(new HashSet<Stage> { new Stage("Stage1") }));

    private readonly IProjectRepository _redisProjectRepository;

    public RedisRepositoryTest(IntegrationTestAppWithRedis testApp) : base(testApp)
    {
        _testApp = testApp;
        _redisProjectRepository = Scope.ServiceProvider.GetRequiredService<IProjectRepository>();
    }

    [Fact]
    public async Task CanSaveAndLoadProject()
    {
        //given
        var project = new Project("project", Stages);
        //and
        project.AddSchedule(Schedule);
        //and
        project.AddDemands(DemandForJava);
        //and
        project.AddChosenResources(NeededResources);
        //and
        project.AddDemandsPerStage(DemandsPerStage);
        //and
        project = await _redisProjectRepository.Save(project);

        //when
        var loaded =
            await _redisProjectRepository.GetById(project.Id);

        //then
        Assert.Equal(NeededResources, loaded.ChosenResources);
        Assert.Equal(Stages, loaded.ParallelizedStages);
        Assert.Equal(Schedule, loaded.Schedule);
        Assert.Equal(DemandForJava, loaded.AllDemands);
        Assert.Equal(DemandsPerStage, loaded.DemandsPerStage);
    }

    [Fact]
    public async Task CanLoadMultipleProjects()
    {
        //given
        var project = new Project("project", Stages);
        var project2 = new Project("project2", Stages);

        //and
        project = await _redisProjectRepository.Save(project);
        project2 = await _redisProjectRepository.Save(project2);

        //when
        var loaded =
            await _redisProjectRepository.FindAllByIdIn(new HashSet<ProjectId>() { project.Id, project2.Id });

        //then
        Assert.Equal(2, loaded.Count);
        var ids = loaded.Select(x => x.Id).ToHashSet();
        CollectionAssert.AreEquivalent(new HashSet<ProjectId>() { project2.Id, project.Id }, ids);
    }

    [Fact]
    public async Task CanLoadAllProjects()
    {
        //given
        var project = new Project("project", Stages);
        var project2 = new Project("project2", Stages);

        //and
        project = await _redisProjectRepository.Save(project);
        project2 = await _redisProjectRepository.Save(project2);

        //when
        var loaded = await _redisProjectRepository.FindAll();

        //then
        Assert.Equal(2, loaded.Count);
        var ids = loaded.Select(x => x.Id).ToHashSet();
        CollectionAssert.AreEquivalent(new HashSet<ProjectId>() { project2.Id, project.Id }, ids);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _testApp.FlushRedisDb();
    }
}

public class IntegrationTestAppWithRedis : IntegrationTestApp
{
    private readonly RedisFixture _redisFixture;

    public IntegrationTestAppWithRedis()
    {
        _redisFixture = new RedisFixture();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configuration = new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", _redisFixture.ConnectionString },
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration)
            .Build();

        builder.UseConfiguration(config);

        builder.ConfigureTestServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped<IProjectRepository, RedisProjectRepository>());
        });
        base.ConfigureWebHost(builder);
    }
    
    public async Task FlushRedisDb()
    {
        await _redisFixture.FlushDb();
    }

    public override async Task InitializeAsync()
    {
        await _redisFixture.InitializeAsync();
        await base.InitializeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _redisFixture.DisposeAsync();
    }
}

public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:5.0.3-alpine")
        .Build();

    public string ConnectionString
    {
        get { return _redis.GetConnectionString(); }
    }

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _redis.DisposeAsync();
    }

    public async Task FlushDb()
    {
        await _redis.ExecScriptAsync("redis.call('FLUSHALL')");
    }
}