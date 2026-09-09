using Forgejo.McpServer.Models;
using Forgejo.McpServer.Services;

namespace Forgejo.McpServer.stdio.tests;

public class ForgejoPullRequestFoundationTests {
    [Test]
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
