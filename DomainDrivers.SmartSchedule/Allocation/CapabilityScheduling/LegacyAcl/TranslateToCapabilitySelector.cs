using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling.LegacyAcl;

public class TranslateToCapabilitySelector
{
    public IList<CapabilitySelector> Translate(EmployeeDataFromLegacyEsbMessage message)
    {
        var employeeSkills = message.SkillsPerformedTogether
            .Select(skills => CapabilitySelector.CanPerformAllAtTheTime(
                skills.Select(x => Capability.Skill(x)).ToHashSet()))
            .ToList();
        var employeeExclusiveSkills = message.ExclusiveSkills
            .Select(skill => CapabilitySelector.CanJustPerform(Capability.Skill(skill)))
            .ToList();
        var employeePermissions = message.Permissions
            .SelectMany(MultiplePermission)
            .ToList();
        //schedule or rewrite if exists;
        return employeeSkills.Concat(employeeExclusiveSkills).Concat(employeePermissions).ToList();
    }

    private IList<CapabilitySelector> MultiplePermission(string permissionLegacyCode)
    {
        var parts = permissionLegacyCode.Split("<>").ToList();
        var permission = parts[0];
        var times = int.Parse(parts[1]);
        return Enumerable
            .Range(0, times)
            .Select(_ => CapabilitySelector.CanJustPerform(Capability.Permission(permission)))
            .ToList();
    }
}