using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public static class CashFlowConfiguration
{
    public static IServiceCollection AddCashFlow(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICashflowDbContext>(
            sp => sp.GetRequiredService<SmartScheduleDbContext>());
        serviceCollection.AddScoped<ICashflowRepository, CashflowRepository>();
        serviceCollection.AddTransient<CashFlowFacade>();
        return serviceCollection;
    }
}

public class CashflowEntityTypeConfiguration : IEntityTypeConfiguration<Cashflow>
{
    public void Configure(EntityTypeBuilder<Cashflow> builder)
    {
        builder.ToTable("cashflows");

        builder.Property<ProjectAllocationsId>(x => x.ProjectId)
            .HasConversion(
                projectId => projectId.Id,
                projectId => new ProjectAllocationsId(projectId))
            .HasColumnName("project_allocations_id");
        builder.HasKey(x => x.ProjectId);

        builder.OwnsOne<Cost>("_cost", ts =>
        {
            ts.Property(t => t.Value).HasColumnName("cost");
        });
        
        builder.OwnsOne<Income>("_income", ts =>
        {
            ts.Property(t => t.Value).HasColumnName("income");
        });
    }
}