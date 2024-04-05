namespace DomainDrivers.SmartSchedule.Allocation;

public interface IProjectAllocationsRepository
{
    Task<IList<ProjectAllocations>> FindAllContainingDate(DateTime when);

    Task<ProjectAllocations?> FindById(ProjectAllocationsId projectId);

    Task<ProjectAllocations> GetById(ProjectAllocationsId projectId);

    Task<ProjectAllocations> Add(ProjectAllocations project);

    Task<ProjectAllocations> Update(ProjectAllocations project);

    Task<IList<ProjectAllocations>> FindAllById(ISet<ProjectAllocationsId> projectIds);

    Task<IList<ProjectAllocations>> FindAll();
}