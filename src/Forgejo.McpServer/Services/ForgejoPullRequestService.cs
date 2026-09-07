using System.Threading;
using Forgejo.McpServer.Models;
using MarkZither.Forgejo.ApiClient;

namespace Forgejo.McpServer.Services;

public sealed class ForgejoPullRequestService {
    private readonly ForgejoClient _client;

    public ForgejoPullRequestService(ForgejoClient client) {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Task<PullRequestOperationResult> CreateAsync(
        PullRequestCreateRequest request,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(request);
        _ = cancellationToken;

        if (!request.IsValid) {
            return Task.FromResult(PullRequestOperationResult.Failure(
                "The pull request request is incomplete or invalid.",
                "validation_error",
                "The repository request is missing a required branch, title, or repository context."));
        }

        return Task.FromResult(PullRequestOperationResult.CreateSuccess(
            $"Pull request validation passed for {request.Owner}/{request.Repository}.",
            pullRequestId: null));
    }

    public static PullRequestOperationResult MapRepositoryFailure(
        string message,
        string errorCode,
        string repositoryReason) =>
        PullRequestOperationResult.Failure(message, errorCode, repositoryReason);
}
