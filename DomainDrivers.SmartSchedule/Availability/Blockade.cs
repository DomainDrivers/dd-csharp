namespace DomainDrivers.SmartSchedule.Availability;

public record Blockade(Owner TakenBy, bool Disabled)
{
    public static Blockade None()
    {
        return new Blockade(Owner.None(), false);
    }

    public static Blockade DisabledBy(Owner owner)
    {
        return new Blockade(owner, true);
    }

    public static Blockade OwnedBy(Owner owner)
    {
        return new Blockade(owner, false);
    }

    public bool CanBeTakenBy(Owner requester)
    {
        return TakenBy.ByNone || TakenBy == requester;
    }

    public bool IsDisabledBy(Owner owner)
    {
        return Disabled && TakenBy == owner;
    }
}