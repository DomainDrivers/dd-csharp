namespace DomainDrivers.SmartSchedule.Tests;

[Collection(nameof(SharedIntegrationTestAppCollection))]
public abstract class IntegrationTestWithSharedApp : IntegrationTest
{
    protected IntegrationTestWithSharedApp(IntegrationTestAppBase app) : base(app)
    {
    }
}

public abstract class IntegrationTest : IDisposable
{
    protected IntegrationTest(IntegrationTestAppBase app)
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