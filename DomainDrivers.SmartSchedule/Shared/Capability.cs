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

    public static ISet<Capability> Skills(params string[] skills)
    {
        return skills.Select(x => Skill(x)).ToHashSet();
    }
    
    public static ISet<Capability> Assets(params string[] assets)
    {
        return assets.Select(x => Asset(x)).ToHashSet();
    }
    
    public static ISet<Capability> Permissions(params string[] permissions)
    {
        return permissions.Select(x => Permission(x)).ToHashSet();
    }
    
    public bool IsOfType(string type)
    {
        return Type == type;
    }
}