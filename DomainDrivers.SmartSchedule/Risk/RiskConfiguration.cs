using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quartz;

namespace DomainDrivers.SmartSchedule.Risk;

public static class RiskConfiguration
{
    public static IServiceCollection AddRisk(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IRiskDbContext>(
            sp => sp.GetRequiredService<SmartScheduleDbContext>());
        serviceCollection.AddTransient<RiskPeriodicCheckSagaDispatcher>();
        serviceCollection.AddScoped<RiskPeriodicCheckSagaRepository>();
        serviceCollection.AddTransient<IRiskPushNotification, RiskPushNotification>();
        serviceCollection.AddTransient<VerifyCriticalResourceAvailableDuringPlanning>();
        serviceCollection.AddTransient<VerifyEnoughDemandsDuringPlanning>();
        serviceCollection.AddTransient<VerifyNeededResourcesAvailableInTimeSlot>();
        serviceCollection.AddQuartz(q =>
        {
            var jobKey = new JobKey("RiskPeriodicCheckSagaWeeklyCheckJob");
            q.AddJob<RiskPeriodicCheckSagaWeeklyCheckJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("RiskPeriodicCheckSagaWeeklyCheckJob-trigger")
                .WithCronSchedule("0 0 12 ? * SUN"));
        });
        return serviceCollection;
    }
}

public class RiskPeriodicCheckSagaEntityTypeConfiguration : IEntityTypeConfiguration<RiskPeriodicCheckSaga>
{
    public void Configure(EntityTypeBuilder<RiskPeriodicCheckSaga> builder)
    {
        builder.ToTable("project_risk_sagas");

        builder.Property<RiskPeriodicCheckSagaId>("_riskSagaId")
            .HasConversion(
                riskPeriodicCheckSagaId => riskPeriodicCheckSagaId.Id,
                riskPeriodicCheckSagaId => new RiskPeriodicCheckSagaId(riskPeriodicCheckSagaId))
            .HasColumnName("project_risk_saga_id");
        builder.HasKey("_riskSagaId");

        builder.Property<ProjectAllocationsId>(x => x.ProjectId)
            .HasConversion(
                projectId => projectId.Id,
                projectId => new ProjectAllocationsId(projectId))
            .HasColumnName("project_allocations_id");
        
        builder.Property<Earnings?>(x => x.Earnings)
            .HasConversion(
                earnings => earnings != null ? (int?)decimal.ToInt32(earnings.Value) : null,
                earnings => earnings != null ? Earnings.Of(earnings.Value) : null)
            .HasColumnName("earnings");

        builder.Property(x => x.MissingDemands)
            .HasColumnName("missing_demands")
            .HasColumnType("jsonb");

        builder.Property(x => x.Deadline)
            .HasColumnName("deadline");
        
        builder.Property("_version")
            .HasColumnName("version")
            .IsConcurrencyToken();
    }
}