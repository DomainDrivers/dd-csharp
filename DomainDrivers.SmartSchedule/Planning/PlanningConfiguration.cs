using DomainDrivers.SmartSchedule.Planning.Parallelization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivers.SmartSchedule.Planning;

public static class PlanningConfiguration
{
    public static IServiceCollection AddPlanning(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPlanningDbContext>(
            sp => sp.GetRequiredService<SmartScheduleDbContext>());
        serviceCollection.AddScoped<IProjectRepository, ProjectRepository>();
        serviceCollection.AddTransient<PlanChosenResources>();
        serviceCollection.AddTransient<StageParallelization>();
        serviceCollection.AddTransient<PlanningFacade>();
        return serviceCollection;
    }
}

public class ProjectEntityTypeConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.Property(p => p.Id)
            .HasConversion(
                projectId => projectId.Id,
                projectId => new ProjectId(projectId))
            .HasColumnName("project_id");
        builder.HasKey(p => p.Id);

        builder.Property("_version")
            .HasColumnName("version")
            .IsConcurrencyToken();

        builder.Property(p => p.Name)
            .HasColumnName("name");

        builder.Property(p => p.ParallelizedStages)
            .HasColumnName("parallelized_stages")
            .HasColumnType("jsonb");

        builder.Property(p => p.DemandsPerStage)
            .HasColumnName("demands_per_stage")
            .HasColumnType("jsonb");

        builder.Property(p => p.AllDemands)
            .HasColumnName("all_demands")
            .HasColumnType("jsonb");

        builder.Property(p => p.ChosenResources)
            .HasColumnName("chosen_resources")
            .HasColumnType("jsonb");

        builder.Property(p => p.Schedule)
            .HasColumnName("schedule")
            .HasColumnType("jsonb");
    }
}