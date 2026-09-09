using System.Threading;
using Forgejo.McpServer.Models;
using MarkZither.Forgejo.ApiClient;
using MarkZither.Forgejo.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

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

            if (createdPullRequest is null) {
                return PullRequestOperationResult.Failure(
                    "Forgejo did not return a pull request after the create request was sent.",
                    "pull_request_not_created",
                    "The repository accepted the request but returned no pull request metadata.");
            }

            return PullRequestOperationResult.CreateSuccess(
                $"Pull request created for {owner}/{repository}.",
                createdPullRequest.Id ?? createdPullRequest.Number);
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

    private static string GetRepositoryMessage(ApiException exception) =>
        !string.IsNullOrWhiteSpace(exception.Message) ? exception.Message : "The Forgejo repository rejected the request.";
}
