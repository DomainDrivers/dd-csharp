namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public class Cashflow
{
    public ProjectAllocationsId ProjectId { get; private set; }
    private Income? _income;
    private Cost? _cost; 

    public Cashflow(ProjectAllocationsId projectId)
    {
        ProjectId = projectId;
    }

    public Earnings Earnings()
    {
        return _income!.Minus(_cost!);
    }

    public void Update(Income income, Cost cost)
    {
        _income = income;
        _cost = cost;
    }
}