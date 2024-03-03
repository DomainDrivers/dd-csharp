using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Shared;
using Demand = DomainDrivers.SmartSchedule.Allocation.Demand;

namespace DomainDrivers.SmartSchedule.Risk;

public interface IRiskPushNotification
{
    void NotifyDemandsSatisfied(ProjectAllocationsId projectId);
    void NotifyAboutAvailability(ProjectAllocationsId projectId, IDictionary<Demand, AllocatableCapabilitiesSummary> available);
    void NotifyProfitableRelocationFound(ProjectAllocationsId projectId, AllocatableCapabilityId allocatableCapabilityId);
    void NotifyAboutPossibleRisk(ProjectAllocationsId projectId);
    void NotifyAboutPossibleRiskDuringPlanning(ProjectId cause, DomainDrivers.SmartSchedule.Planning.Demands demands);
    void NotifyAboutCriticalResourceNotAvailable(ProjectId cause, ResourceId criticalResource, TimeSlot timeSlot);
    void NotifyAboutResourcesNotAvailable(ProjectId projectId, ISet<ResourceId> notAvailable);
}

public class RiskPushNotification : IRiskPushNotification
{
    public void NotifyDemandsSatisfied(ProjectAllocationsId projectId)
    {
    }

    public void NotifyAboutAvailability(ProjectAllocationsId projectId, IDictionary<Demand, AllocatableCapabilitiesSummary> available)
    {
    }

    public void NotifyProfitableRelocationFound(ProjectAllocationsId projectId, AllocatableCapabilityId allocatableCapabilityId)
    {
    }

    public void NotifyAboutPossibleRisk(ProjectAllocationsId projectId)
    {
    }

    public void NotifyAboutPossibleRiskDuringPlanning(ProjectId cause, DomainDrivers.SmartSchedule.Planning.Demands demands)
    {
    }

    public void NotifyAboutCriticalResourceNotAvailable(ProjectId cause, ResourceId criticalResource, TimeSlot timeSlot)
    {
    }

    public void NotifyAboutResourcesNotAvailable(ProjectId projectId, ISet<ResourceId> notAvailable)
    {
    }
}