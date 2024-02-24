using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Shared;

public class CapabilitySelectorTest
{
    static readonly Capability Rust = new Capability("RUST", "SKILL");
    static readonly Capability BeingAnAdmin = new Capability("ADMIN", "PERMISSION");
    static readonly Capability Java = new Capability("JAVA", "SKILL");

    [Fact]
    public void AllocatableResourceCanPerformOnlyOneOfPresentCapabilities()
    {
        //given
        var adminOrRust = CapabilitySelector.CanPerformOneOf(new HashSet<Capability>() { BeingAnAdmin, Rust });

        //expect
        Assert.True(adminOrRust.CanPerform(BeingAnAdmin));
        Assert.True(adminOrRust.CanPerform(Rust));
        Assert.False(adminOrRust.CanPerform(new HashSet<Capability>() { Rust, BeingAnAdmin }));
        Assert.False(adminOrRust.CanPerform(new Capability("JAVA", "SKILL")));
        Assert.False(adminOrRust.CanPerform(new Capability("LAWYER", "PERMISSION")));
    }

    [Fact]
    public void AllocatableResourceCanPerformSimultaneousCapabilities()
    {
        //given
        var adminAndRust = CapabilitySelector.CanPerformAllAtTheTime(new HashSet<Capability>() { BeingAnAdmin, Rust });

        //expect
        Assert.True(adminAndRust.CanPerform(BeingAnAdmin));
        Assert.True(adminAndRust.CanPerform(Rust));
        Assert.True(adminAndRust.CanPerform(new HashSet<Capability>() { Rust, BeingAnAdmin }));
        Assert.False(adminAndRust.CanPerform(new HashSet<Capability>() { Rust, BeingAnAdmin, Java }));
        Assert.False(adminAndRust.CanPerform(Java));
        Assert.False(adminAndRust.CanPerform(new Capability("LAWYER", "PERMISSION")));
    }
}