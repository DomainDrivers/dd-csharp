using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource.Employee;

public class EmployeeFacade
{
    private readonly EmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeFacade(EmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeSummary> FindEmployee(EmployeeId employeeId)
    {
        return await _employeeRepository.FindSummary(employeeId);
    }

    public async Task<IList<Capability>> FindAllCapabilities()
    {
        return await _employeeRepository.FindAllCapabilities();
    }

    public async Task<EmployeeId> AddEmployee(string name, string lastName, Seniority seniority,
        ISet<Capability> skills, ISet<Capability> permissions)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var employeeId = EmployeeId.NewOne();
            var capabilities = skills.Concat(permissions).ToHashSet();
            var employee = new Employee(employeeId, name, lastName, seniority, capabilities);
            await _employeeRepository.Add(employee);
            return employeeId;
        });
    }

    //add vacation
    // calls availability
    //add sick leave
    // calls availability
    //change skills
}