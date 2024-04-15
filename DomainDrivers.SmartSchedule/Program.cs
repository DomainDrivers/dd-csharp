using System.Data;
using System.Text.Json;
using DomainDrivers.SmartSchedule;
using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Optimization;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Resource;
using DomainDrivers.SmartSchedule.Resource.Device;
using DomainDrivers.SmartSchedule.Resource.Employee;
using DomainDrivers.SmartSchedule.Risk;
using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Quartz;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString!));

var dataSource = new NpgsqlDataSourceBuilder(postgresConnectionString)
    .ConfigureJsonOptions(new JsonSerializerOptions() { IgnoreReadOnlyProperties = true, IgnoreReadOnlyFields = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase})
    .EnableDynamicJson()
    .Build();
builder.Services.AddDbContext<SmartScheduleDbContext>(options => { options.UseNpgsql(dataSource); });
builder.Services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<SmartScheduleDbContext>().Database.GetDbConnection());
builder.Services.AddShared();
builder.Services.AddPlanning();
builder.Services.AddAvailability();
builder.Services.AddAllocation();
builder.Services.AddCashFlow();
builder.Services.AddEmployee();
builder.Services.AddDevice();
builder.Services.AddResource();
builder.Services.AddCapabilityPlanning();
builder.Services.AddOptimization();
builder.Services.AddSimulation();
builder.Services.AddRisk();
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

app.Run();

public partial class Program;