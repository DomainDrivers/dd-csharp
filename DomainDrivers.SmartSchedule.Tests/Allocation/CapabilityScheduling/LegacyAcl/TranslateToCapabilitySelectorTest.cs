using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling.LegacyAcl;
using DomainDrivers.SmartSchedule.Shared;
using NUnit.Framework.Legacy;
using static DomainDrivers.SmartSchedule.Shared.CapabilitySelector;
using static DomainDrivers.SmartSchedule.Shared.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Allocation.CapabilityScheduling.LegacyAcl;

public class TranslateToCapabilitySelectorTest
{
    [Fact]
    public void TranslateLegacyEsbMessageToCapabilitySelectorModel()
    {
        //given
        var legacyPermissions = new List<string> { "ADMIN<>2", "ROOT<>1" };
        var legacySkillsPerformedTogether = new List<IList<string>>
        {
            new List<string> { "JAVA", "CSHARP", "PYTHON" },
            new List<string> { "RUST", "CSHARP", "PYTHON" }
        };
        var legacyExclusiveSkills = new List<string> { "YT DRAMA COMMENTS" };

        //when
        var result = Translate(legacySkillsPerformedTogether, legacyExclusiveSkills, legacyPermissions);

        //then
        CollectionAssert.AreEquivalent(new List<CapabilitySelector>
            {
                CanPerformOneOf(new HashSet<Capability> { Skill("YT DRAMA COMMENTS") }),
                CapabilitySelector.CanPerformAllAtTheTime(Capability.Skills("JAVA", "CSHARP", "PYTHON")),
                CapabilitySelector.CanPerformAllAtTheTime(Capability.Skills("RUST", "CSHARP", "PYTHON")),
                CanPerformOneOf(new HashSet<Capability> { Permission("ADMIN") }),
                CanPerformOneOf(new HashSet<Capability> { Permission("ADMIN") }),
                CanPerformOneOf(new HashSet<Capability> { Permission("ROOT") })
            },
            result
        );
    }

    [Fact]
    public void ZeroMeansNoPermissionNowhere()
    {
        var legacyPermissions = new List<string> { "ADMIN<>0" };

        //when
        var result = Translate(new List<IList<string>>(), new List<string>(), legacyPermissions);

        //then
        Assert.Empty(result);
    }

    private IList<CapabilitySelector> Translate(IList<IList<string>> legacySkillsPerformedTogether,
        IList<string> legacyExclusiveSkills, IList<string> legacyPermissions)
    {
        return new TranslateToCapabilitySelector().Translate(new EmployeeDataFromLegacyEsbMessage(Guid.NewGuid(),
            legacySkillsPerformedTogether, legacyExclusiveSkills, legacyPermissions, TimeSlot.Empty()));
    }
}