using System.Reflection;
using DomainDrivers.SmartSchedule.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace DomainDrivers.SmartSchedule.Tests;

[CollectionDefinition(nameof(SharedIntegrationTestAppCollection))]
public class SharedIntegrationTestAppCollection : ICollectionFixture<IntegrationTestApp>;

public class IntegrationTestApp : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgresFixture _postgres;

    public IntegrationTestApp()
    {
        _postgres = new PostgresFixture();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Postgres", _postgres.ConnectionString },
            })
            .Build();
        builder.UseConfiguration(config);
        builder.ConfigureTestServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped<IEventsPublisher>(_ => Substitute.For<IEventsPublisher>()));
        });
        base.ConfigureWebHost(builder);
    }

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    public string ConnectionString
    {
        get { return _postgres.GetConnectionString(); }
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await ExecuteResourceScripts();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    private async Task ExecuteResourceScripts()
    {
        var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var resourcesPath = Path.Combine(dirPath, "Resources");
        
        var files = Directory.GetFiles(resourcesPath, "*.sql");
            
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            await _postgres.ExecScriptAsync(content);
        }
    }
}