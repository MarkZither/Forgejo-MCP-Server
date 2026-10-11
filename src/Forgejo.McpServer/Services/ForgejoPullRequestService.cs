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

    public async Task<PullRequestOperationResult> AddCommentAsync(
        PullRequestCommentRequest request,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.IsValid) {
            return PullRequestOperationResult.Failure(
                "The pull request comment request is incomplete or invalid.",
                "validation_error",
                "The repository request is missing a valid pull request number or comment body.");
        }

        try {
            var owner = request.Owner.Trim();
            var repository = request.Repository.Trim();
            var requestPayload = request.ToCreateIssueCommentOption();

            var createdComment = await _client.Repos[owner][repository].Issues[request.PullRequestNumber].Comments
                .PostAsync(requestPayload, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            Log.Debug("AddPullRequestComment response: {@CreatedComment}", createdComment);

            if (createdComment is null) {
                return PullRequestOperationResult.Failure(
                    "Forgejo did not return a comment after the request was sent.",
                    "comment_not_created",
                    "The repository accepted the request but returned no comment metadata.");
            }

            return PullRequestOperationResult.CreateSuccess(
                $"Comment added to pull request #{request.PullRequestNumber} in {owner}/{repository}.",
                createdComment.Id ?? request.PullRequestNumber);
        } catch (APIValidationError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the comment because the pull request or payload was invalid.",
                "validation_error",
                GetRepositoryMessage(ex));
        } catch (APINotFound ex) {
            return MapRepositoryFailure(
                "Forgejo could not find the pull request discussion for this repository.",
                "not_found",
                GetRepositoryMessage(ex));
        } catch (APIForbiddenError ex) {
            return MapRepositoryFailure(
                "Forgejo blocked the comment because the token was not allowed to comment on this pull request.",
                "forbidden",
                GetRepositoryMessage(ex));
        } catch (APIUnauthorizedError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the comment because the credential was not authorized.",
                "unauthorized",
                GetRepositoryMessage(ex));
        } catch (APIRepoArchivedError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the comment because the repository is archived.",
                "repository_archived",
                GetRepositoryMessage(ex));
        } catch (APIError ex) {
            return MapRepositoryFailure(
                "Forgejo reported a repository error while adding the pull request comment.",
                "repository_error",
                GetRepositoryMessage(ex));
        } catch (Exception ex) when (ex is not OperationCanceledException) {
            return PullRequestOperationResult.Failure(
                "An unexpected error occurred while adding the pull request comment.",
                "unexpected_error",
                ex.Message);
        }
    }

    public async Task<PullRequestDiscussionResult> GetDiscussionAsync(
        string owner,
        string repository,
        long pullRequestNumber,
        CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(owner) ||
            string.IsNullOrWhiteSpace(repository) ||
            pullRequestNumber <= 0) {
            return PullRequestDiscussionResult.Failure(
                "The pull request discussion request is incomplete or invalid.",
                "validation_error",
                "The repository request is missing a valid repository or pull request number.");
        }

        try {
            var normalizedOwner = owner.Trim();
            var normalizedRepository = repository.Trim();

            var pullRequest = await _client.Repos[normalizedOwner][normalizedRepository].Pulls[pullRequestNumber]
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (pullRequest is null) {
                return PullRequestDiscussionResult.Failure(
                    "Forgejo could not find the pull request discussion for the requested repository state.",
                    "not_found",
                    "The repository did not return a pull request for the requested number.");
            }

            var comments = await _client.Repos[normalizedOwner][normalizedRepository].Issues[pullRequestNumber].Comments
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var visibleComments = comments?
                .Select(comment => comment.Body)
                .Where(body => !string.IsNullOrWhiteSpace(body))
                .Select(body => body!.Trim())
                .ToList() ?? new List<string>();

            return PullRequestDiscussionResult.CreateSuccess(
                $"Retrieved discussion for pull request #{pullRequestNumber} in {normalizedOwner}/{normalizedRepository}.",
                pullRequest.State,
                visibleComments);
        } catch (APIValidationError ex) {
            return PullRequestDiscussionResult.Failure(
                "Forgejo rejected the discussion fetch because the pull request reference was invalid.",
                "validation_error",
                GetRepositoryMessage(ex));
        } catch (APINotFound ex) {
            return PullRequestDiscussionResult.Failure(
                "Forgejo could not find the pull request discussion for this repository.",
                "not_found",
                GetRepositoryMessage(ex));
        } catch (APIForbiddenError ex) {
            return PullRequestDiscussionResult.Failure(
                "Forgejo blocked the discussion lookup because the token was not allowed to read it.",
                "forbidden",
                GetRepositoryMessage(ex));
        } catch (APIUnauthorizedError ex) {
            return PullRequestDiscussionResult.Failure(
                "Forgejo rejected the discussion lookup because the credential was not authorized.",
                "unauthorized",
                GetRepositoryMessage(ex));
        } catch (APIRepoArchivedError ex) {
            return PullRequestDiscussionResult.Failure(
                "Forgejo rejected the discussion lookup because the repository is archived.",
                "repository_archived",
                GetRepositoryMessage(ex));
        } catch (APIError ex) {
            return PullRequestDiscussionResult.Failure(
                "Forgejo reported a repository error while reading the pull request discussion.",
                "repository_error",
                GetRepositoryMessage(ex));
        } catch (Exception ex) when (ex is not OperationCanceledException) {
            return PullRequestDiscussionResult.Failure(
                "An unexpected error occurred while reading the pull request discussion.",
                "unexpected_error",
                ex.Message);
        }
    }

    public async Task<PullRequestOperationResult> MergeAsync(
        PullRequestMergeRequest request,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.IsValid) {
            return PullRequestOperationResult.Failure(
                "The pull request merge request is incomplete or invalid.",
                "validation_error",
                "The repository request is missing a valid pull request number or a supported merge strategy.");
        }

        try {
            var owner = request.Owner.Trim();
            var repository = request.Repository.Trim();

            var pullRequest = await _client.Repos[owner][repository].Pulls[request.PullRequestNumber]
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (pullRequest is null) {
                return PullRequestOperationResult.Failure(
                    "Forgejo could not find the pull request to merge for the requested repository.",
                    "not_found",
                    "The repository did not return a pull request for the requested number.");
            }

            var mergeReadinessFailure = ValidateMergeReadiness(pullRequest);
            if (mergeReadinessFailure is not null) {
                return mergeReadinessFailure;
            }

            var requestPayload = request.ToMergePullRequestOption();
            var mergeResult = await _client.Repos[owner][repository].Pulls[request.PullRequestNumber].Merge
                .PostAsync(requestPayload, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            Log.Debug("MergePullRequest response: {@MergeResult}", mergeResult);

            if (mergeResult is null) {
                return PullRequestOperationResult.Failure(
                    "Forgejo did not return a merge result after the merge request was sent.",
                    "merge_not_completed",
                    "The repository accepted the merge request but did not return a merge outcome.");
            }

            return PullRequestOperationResult.CreateSuccess(
                $"Pull request #{request.PullRequestNumber} in {owner}/{repository} was merged using the {request.MergeStrategy} strategy.",
                request.PullRequestNumber,
                merged: true);
        } catch (APIValidationError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the pull request merge because the request was invalid.",
                "validation_error",
                GetRepositoryMessage(ex));
        } catch (APINotFound ex) {
            return MapRepositoryFailure(
                "Forgejo could not find the pull request to merge for this repository.",
                "not_found",
                GetRepositoryMessage(ex));
        } catch (APIForbiddenError ex) {
            return MapRepositoryFailure(
                "Forgejo blocked the merge because the token was not allowed to perform it.",
                "forbidden",
                GetRepositoryMessage(ex));
        } catch (APIUnauthorizedError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the merge because the credential was not authorized.",
                "unauthorized",
                GetRepositoryMessage(ex));
        } catch (APIRepoArchivedError ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the merge because the repository is archived.",
                "repository_archived",
                GetRepositoryMessage(ex));
        } catch (APIError ex) {
            return MapRepositoryFailure(
                "Forgejo blocked the merge because repository policy or merge checks rejected it.",
                "merge_blocked",
                GetRepositoryMessage(ex));
        } catch (Exception ex) when (ex is not OperationCanceledException) {
            return PullRequestOperationResult.Failure(
                "An unexpected error occurred while merging the pull request.",
                "unexpected_error",
                ex.Message);
        }
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
                "Forgejo reported a repository policy or validation rejection while creating the pull request.",
                GetRepositoryFailureCode(ex, defaultCode: "repository_error"),
                GetRepositoryMessage(ex));
        } catch (ApiException ex) {
            return MapRepositoryFailure(
                "Forgejo rejected the pull request because repository policy or permissions blocked it.",
                GetRepositoryFailureCode(ex, defaultCode: "repository_error"),
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

    private static PullRequestOperationResult? ValidateMergeReadiness(
        MarkZither.Forgejo.ApiClient.Models.PullRequest pullRequest) {
        if (pullRequest.Merged == true || string.Equals(pullRequest.State, "closed", StringComparison.OrdinalIgnoreCase)) {
            return PullRequestOperationResult.Failure(
                "Forgejo will not merge a pull request that has already been merged or closed.",
                "already_merged",
                "The pull request is not in an open state for a merge operation.");
        }

        if (pullRequest.Draft == true) {
            return PullRequestOperationResult.Failure(
                "Forgejo will not merge a draft pull request until it is marked ready for review.",
                "merge_blocked",
                "The pull request is still a draft and has not met the review requirements to be mergeable.");
        }

        if (pullRequest.IsLocked == true) {
            return PullRequestOperationResult.Failure(
                "Forgejo will not merge a locked pull request while the discussion is restricted.",
                "merge_blocked",
                "The pull request is locked and cannot be merged until repository moderators unlock it.");
        }

        if (pullRequest.Mergeable == false) {
            return PullRequestOperationResult.Failure(
                "Forgejo will not merge a pull request that is not currently mergeable.",
                "merge_blocked",
                "The pull request is not currently mergeable because it has conflicts or repository checks are still pending.");
        }

        return null;
    }

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

    private static string GetRepositoryMessage(ApiException exception) {
        var message = exception.Message ?? string.Empty;

        if (string.IsNullOrWhiteSpace(message)) {
            return "The Forgejo repository rejected the request.";
        }

        if (message.Contains("no error factory is registered", StringComparison.OrdinalIgnoreCase)
            && message.Contains("403", StringComparison.OrdinalIgnoreCase)) {
            return "Forgejo blocked the pull request because branch protection rules or repository policy rejected it.";
        }

        return message;
    }

    private static string GetRepositoryFailureCode(ApiException exception, string defaultCode) {
        var message = exception.Message ?? string.Empty;
        var typeName = exception.GetType().Name ?? string.Empty;

        if (typeName.Contains("Forbidden", StringComparison.OrdinalIgnoreCase)
            || message.Contains("403", StringComparison.OrdinalIgnoreCase)
            || message.Contains("no error factory is registered", StringComparison.OrdinalIgnoreCase)
            || message.Contains("forbidden", StringComparison.OrdinalIgnoreCase)
            || message.Contains("not allowed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("branch protection", StringComparison.OrdinalIgnoreCase)
            || message.Contains("protection", StringComparison.OrdinalIgnoreCase)
            || message.Contains("required review", StringComparison.OrdinalIgnoreCase)
            || message.Contains("policy", StringComparison.OrdinalIgnoreCase)
            || message.Contains("denied", StringComparison.OrdinalIgnoreCase)) {
            return "forbidden";
        }

        return defaultCode;
    }
}
