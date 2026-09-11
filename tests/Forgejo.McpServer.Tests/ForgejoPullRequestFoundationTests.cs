using Forgejo.McpServer.Configuration;
using Forgejo.McpServer.Models;
using Forgejo.McpServer.Services;
using Microsoft.Extensions.Configuration;

namespace Forgejo.McpServer.stdio.tests;

public class ForgejoPullRequestFoundationTests {
    private static bool HasRealIntegrationConfiguration() {
        var baseUrl = ForgejoTestSettings.BaseUrl;
        var token = ForgejoTestSettings.Token;

        return !string.IsNullOrWhiteSpace(baseUrl)
            && !string.IsNullOrWhiteSpace(token)
            && !string.Equals(baseUrl, "https://forgejo.home", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(baseUrl, "https://forgejo.example.com", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(token, "temptoken", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(token, "secret-token", StringComparison.OrdinalIgnoreCase);
    }

    [Test]
    [Category("Unit")]
    public async Task PullRequestCreateRequest_RejectsMissingTitle() {
        var request = new PullRequestCreateRequest(
            "acme",
            "demo-repo",
            "feature/test",
            "main",
            " ");

        await Assert.That(request.IsValid).IsFalse();
    }

    [Test]
    [Category("Unit")]
    [Arguments("feature/test", "main", true)]
    [Arguments("release-2026.09", "develop", true)]
    [Arguments("bugfix/issue-123", "main", true)]
    [Arguments("feature invalid", "main", false)]
    [Arguments("feature@{oops}", "main", false)]
    [Arguments("feature..bad", "main", false)]
    [Arguments(".bad", "main", false)]
    [Arguments("main", "main", false)]
    public async Task PullRequestCreateRequest_ValidatesBranchPairs(string headBranch, string baseBranch, bool expectedIsValid) {
        var request = new PullRequestCreateRequest(
            "acme",
            "demo-repo",
            headBranch,
            baseBranch,
            "PR with branch validation");

        await Assert.That(request.IsValid).IsEqualTo(expectedIsValid);
    }

    [Test]
    [Category("Integration")]
    public async Task ForgejoPullRequestService_Validation_CreatePullRequest_UsesConfiguredRepo() {
        if (!HasRealIntegrationConfiguration()) {
            return;
        }

        var baseUrl = ForgejoTestSettings.BaseUrl;
        var token = ForgejoTestSettings.Token;
        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(baseUrl, token));
        var request = new PullRequestCreateRequest(
            "mark",
            "MCPTest",
            "tomerge",
            "main",
            "Integration validation PR",
            "Generated for end-to-end validation of the Forgejo MCP create PR flow.");

        var result = await service.CreateAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.PullRequestId).IsNotNull();
    }

    [Test]
    [Category("Integration")]
    public async Task ForgejoPullRequestService_Validation_RejectsInvalidBranchNames() {
        if (!HasRealIntegrationConfiguration()) {
            return;
        }

        var baseUrl = ForgejoTestSettings.BaseUrl;
        var token = ForgejoTestSettings.Token;
        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(baseUrl, token));
        var request = new PullRequestCreateRequest(
            "mark",
            "MCPTest",
            "feature invalid",
            "main",
            "This should be rejected before any repository call");

        var result = await service.CreateAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("validation_error");
    }

    [Test]
    public async Task ForgejoOptions_BindsStandardForgejoConfigurationKeys() {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["Forgejo:BaseUrl"] = "https://forgejo.example.com",
                ["Forgejo:Token"] = "secret-token"
            })
            .Build();

        var options = configuration.GetSection("Forgejo").Get<ForgejoOptions>();

        await Assert.That(options).IsNotNull();
        await Assert.That(options!.BaseUrl).IsEqualTo("https://forgejo.example.com");
        await Assert.That(options.Token).IsEqualTo("secret-token");
    }

    [Test]
    public async Task PullRequestOperationResult_ReportsRepositoryFailure() {
        var result = PullRequestOperationResult.Failure(
            "Branch protection blocks merge.",
            "branch_protection",
            "Required review not satisfied.");

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("branch_protection");
        await Assert.That(result.RepositoryReason).IsEqualTo("Required review not satisfied.");
    }

    [Test]
    public async Task ForgejoPullRequestService_RejectsSameBranchPullRequest() {
        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create("https://forgejo.example.com", "secret-token"));
        var request = new PullRequestCreateRequest(
            "acme",
            "demo-repo",
            "main",
            "main",
            "Same branch pull request");

        var result = await service.CreateAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("validation_error");
        await Assert.That(result.RepositoryReason).Contains("source branch");
    }

    [Test]
    public async Task ForgejoClientFactory_UsesProvidedBaseUrlAndToken() {
        var client = ForgejoClientFactory.Create("https://forgejo.example.com", "secret-token");

        await Assert.That(client).IsNotNull();
    }
}
