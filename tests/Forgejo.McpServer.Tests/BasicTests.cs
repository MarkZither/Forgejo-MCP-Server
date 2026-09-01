namespace Forgejo.McpServer.stdio.tests;

public class BasicTests {
    [Before(Class)]
#pragma warning disable IDE0060 // Remove unused parameter
    public static Task BeforeClass(ClassHookContext context) {
        // Runs once before all tests in this class
        return Task.CompletedTask;
    }
#pragma warning restore IDE0060 // Remove unused parameter

    [After(Class)]
#pragma warning disable IDE0060 // Remove unused parameter
    public static Task AfterClass(ClassHookContext context) {
        // Runs once after all tests in this class
        return Task.CompletedTask;
    }
#pragma warning restore IDE0060 // Remove unused parameter

    [Before(Test)]
#pragma warning disable IDE0060 // Remove unused parameter
    public Task BeforeTest(TestContext context) {
        // Runs before each test in this class
        return Task.CompletedTask;
    }
#pragma warning restore IDE0060 // Remove unused parameter

    [After(Test)]
#pragma warning disable IDE0060 // Remove unused parameter
    public Task AfterTest(TestContext context) {
        // Runs after each test in this class
        return Task.CompletedTask;
    }
#pragma warning restore IDE0060 // Remove unused parameter

    [Test]
    public async Task Add_ReturnsSum() {
        var calculator = new Calculator();

        var result = calculator.Add(1, 2);

        await Assert.That(result).IsEqualTo(3);
    }

    [Test]
    public async Task Divide_ByZero_ThrowsException() {
        var calculator = new Calculator();

        var action = () => calculator.Divide(1, 0);

        await Assert.That(action).ThrowsException()
            .WithMessage("Attempted to divide by zero.");
    }
}
