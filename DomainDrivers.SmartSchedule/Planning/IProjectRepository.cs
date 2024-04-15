namespace DomainDrivers.SmartSchedule.Planning;

public interface IProjectRepository
{
    Task<Project> GetById(ProjectId projectId);

    Task<Project> Save(Project project);

    Task<IList<Project>> FindAllByIdIn(ISet<ProjectId> projectIds);

    Task<IList<Project>> FindAll();
}