using System.Threading;
using Forgejo.McpServer.Models;
using MarkZither.Forgejo.ApiClient;
using MarkZither.Forgejo.ApiClient.Models;
using Microsoft.Kiota.Abstractions;
using Serilog;

namespace Forgejo.McpServer.Services;

public sealed class ForgejoPullRequestService {
    private readonly ForgejoClient _client;

    public ForgejoPullRequestService(ForgejoClient client) {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<PullRequestOperationResult> CreateAsync(
        PullRequestCreateRequest request,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.IsValid) {
            if (string.Equals(request.HeadBranch.Trim(), request.BaseBranch.Trim(), StringComparison.OrdinalIgnoreCase)) {
                return PullRequestOperationResult.Failure(
                    "The pull request request is invalid because the source branch and target branch are the same.",
                    "validation_error",
                    "The source branch and target branch must be different for a pull request.");
            }

            return PullRequestOperationResult.Failure(
                "The pull request request is incomplete or invalid.",
                "validation_error",
                "The repository request is missing a required branch, title, or repository context.");
        }

        try {
            var owner = request.Owner.Trim();
            var repository = request.Repository.Trim();
            var requestPayload = request.ToCreatePullRequestOption();

            var createdPullRequest = await _client.Repos[owner][repository].Pulls
                .PostAsync(requestPayload, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            Log.Debug("CreatePullRequest response: {@CreatedPullRequest}", createdPullRequest);

            if (createdPullRequest is null) {
                return PullRequestOperationResult.Failure(
                    "Forgejo did not return a pull request after the create request was sent.",
                    "pull_request_not_created",
                    "The repository accepted the request but returned no pull request metadata.");
            }

            var visiblePullRequest = await FindVisiblePullRequestAsync(
                owner,
                repository,
                request,
                createdPullRequest,
                cancellationToken).ConfigureAwait(false);

            if (visiblePullRequest is null) {
                return PullRequestOperationResult.Failure(
                    "Forgejo reported a pull request was created, but the repository did not expose a matching pull request.",
                    "pull_request_not_visible",
                    "The create response was returned without a matching pull request being visible in the repository.");
            }

            return PullRequestOperationResult.CreateSuccess(
                $"Pull request created for {owner}/{repository}.",
                visiblePullRequest.Id ?? visiblePullRequest.Number ?? createdPullRequest.Id ?? createdPullRequest.Number);
        } catch (APIValidationError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the pull request because the branch or payload was invalid.",
                "validation_error",
                GetRepositoryMessage(ex));
        } catch (APINotFound ex) {
            return MapRepositoryFailure(
                "Forgejo could not find the repository or branch for this pull request.",
                "not_found",
                GetRepositoryMessage(ex));
        } catch (APIForbiddenError ex) {
            return MapRepositoryFailure(
                "Forgejo blocked the pull request because the token was not allowed to create it.",
                "forbidden",
                GetRepositoryMessage(ex));
        } catch (APIUnauthorizedError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the pull request because the credential was not authorized.",
                "unauthorized",
                GetRepositoryMessage(ex));
        } catch (APIRepoArchivedError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the pull request because the repository is archived.",
                "repository_archived",
                GetRepositoryMessage(ex));
        } catch (APIError ex) {
            return MapRepositoryFailure(
                "Forgejo reported a repository error while creating the pull request.",
                "repository_error",
                GetRepositoryMessage(ex));
        } catch (Exception ex) when (ex is not OperationCanceledException) {
            return PullRequestOperationResult.Failure(
                "An unexpected error occurred while creating the pull request.",
                "unexpected_error",
                ex.Message);
        }
    }

    public static PullRequestOperationResult MapRepositoryFailure(
        string message,
        string errorCode,
        string repositoryReason) =>
        PullRequestOperationResult.Failure(message, errorCode, repositoryReason);

    private async Task<MarkZither.Forgejo.ApiClient.Models.PullRequest?> FindVisiblePullRequestAsync(
        string owner,
        string repository,
        PullRequestCreateRequest request,
        MarkZither.Forgejo.ApiClient.Models.PullRequest createdPullRequest,
        CancellationToken cancellationToken) {
        var pullRequests = await _client.Repos[owner][repository].Pulls
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var expectedHead = request.HeadBranch.Trim();
        var expectedBase = request.BaseBranch.Trim();
        var expectedTitle = request.Title.Trim();

        return pullRequests?
            .FirstOrDefault(pr =>
                MatchesPullRequest(pr, expectedHead, expectedBase, expectedTitle, createdPullRequest));
    }

    private static bool MatchesPullRequest(
        MarkZither.Forgejo.ApiClient.Models.PullRequest pullRequest,
        string expectedHead,
        string expectedBase,
        string expectedTitle,
        MarkZither.Forgejo.ApiClient.Models.PullRequest createdPullRequest) {
        var sameNumber = createdPullRequest.Number is not null && pullRequest.Number == createdPullRequest.Number;
        var sameId = createdPullRequest.Id is not null && pullRequest.Id == createdPullRequest.Id;
        var sameBranchAndTitle = string.Equals(pullRequest.Head?.Ref, expectedHead, StringComparison.OrdinalIgnoreCase)
            && string.Equals(pullRequest.Base?.Ref, expectedBase, StringComparison.OrdinalIgnoreCase)
            && string.Equals(pullRequest.Title, expectedTitle, StringComparison.OrdinalIgnoreCase);

        return sameNumber || sameId || sameBranchAndTitle;
    }

    private static string GetRepositoryMessage(ApiException exception) =>
        !string.IsNullOrWhiteSpace(exception.Message) ? exception.Message : "The Forgejo repository rejected the request.";
}
