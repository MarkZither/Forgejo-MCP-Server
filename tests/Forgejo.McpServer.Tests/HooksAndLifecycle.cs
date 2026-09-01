using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace Forgejo.McpServer.stdio.tests;

public static class GlobalHooks {
    [Before(TestSession)]
#pragma warning disable IDE0060 // Remove unused parameter
    public static Task BeforeTestSession(TestSessionContext context) {
        // Runs once before all tests - e.g. start a test container, seed a database
        return Task.CompletedTask;
    }
#pragma warning restore IDE0060 // Remove unused parameter

    [After(TestSession)]
#pragma warning disable IDE0060 // Remove unused parameter
    public static Task AfterTestSession(TestSessionContext context) {
        // Runs once after all tests - e.g. stop containers, clean up resources
        return Task.CompletedTask;
    }
#pragma warning restore IDE0060 // Remove unused parameter

}
