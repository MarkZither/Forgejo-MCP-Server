namespace Forgejo.McpServer.Models;

public sealed class PullRequestDiscussionResult {
    private PullRequestDiscussionResult(
        bool success,
        string? message,
        string? errorCode,
        string? repositoryReason,
        string? pullRequestState = null,
        IReadOnlyList<string>? comments = null) {
        Success = success;
        Message = message;
        ErrorCode = errorCode;
        RepositoryReason = repositoryReason;
        PullRequestState = pullRequestState;
        Comments = comments ?? Array.Empty<string>();
    }

    public bool Success { get; }
    public string? Message { get; }
    public string? ErrorCode { get; }
    public string? RepositoryReason { get; }
    public string? PullRequestState { get; }
    public IReadOnlyList<string> Comments { get; }

    public static PullRequestDiscussionResult CreateSuccess(
        string message,
        string? pullRequestState,
        IReadOnlyList<string>? comments = null) =>
        new(success: true, message: message, errorCode: null, repositoryReason: null, pullRequestState: pullRequestState, comments: comments);

    public static PullRequestDiscussionResult Failure(
        string message,
        string errorCode,
        string repositoryReason,
        string? pullRequestState = null,
        IReadOnlyList<string>? comments = null) =>
        new(success: false, message: message, errorCode: errorCode, repositoryReason: repositoryReason, pullRequestState: pullRequestState, comments: comments);
}
