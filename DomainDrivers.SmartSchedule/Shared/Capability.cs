namespace DomainDrivers.SmartSchedule.Shared;

public record Capability(string Name, string Type)
{
    public static Capability Skill(string name)
    {
        return new Capability(name, "SKILL");
    }

    public static Capability Permission(string name)
    {
        return new Capability(name, "PERMISSION");
    }

    public static Capability Asset(string asset)
    {
        return new Capability(asset, "ASSET");
    }

    public static IList<Capability> Skills(params string[] skills)
    {
        return skills.Select(x => Capability.Skill(x)).ToList();
    }
}