namespace DomainDrivers.SmartSchedule.Planning;

public interface IProjectRepository
{
    Task<Project> GetById(ProjectId projectId);

    Task<Project> Add(Project project);

    Task<Project> Update(Project project);

    Task<IList<Project>> FindAllByIdIn(ISet<ProjectId> projectIds);

    Task<IList<Project>> FindAll();
}