namespace DomainDrivers.SmartSchedule.Tests;

[Collection(nameof(SharedIntegrationTestAppCollection))]
public abstract class IntegrationTest : IDisposable
{
    protected IntegrationTest(IntegrationTestApp app)
    {
        // One scope per test to match java's behavior.
        Scope = app.Services.CreateScope();
    }

    protected IServiceScope Scope { get; }

    public void Dispose()
    {
        Scope.Dispose();
    }
}