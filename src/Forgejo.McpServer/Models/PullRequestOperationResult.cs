namespace Forgejo.McpServer.Models;

public sealed class PullRequestOperationResult {
    private PullRequestOperationResult(
        bool success,
        string? message,
        string? errorCode,
        string? repositoryReason,
        long? pullRequestId = null) {
        Success = success;
        Message = message;
        ErrorCode = errorCode;
        RepositoryReason = repositoryReason;
        PullRequestId = pullRequestId;
    }

    public bool Success { get; }
    public string? Message { get; }
    public string? ErrorCode { get; }
    public string? RepositoryReason { get; }
    public long? PullRequestId { get; }

    public static PullRequestOperationResult CreateSuccess(string message, long? pullRequestId = null) =>
        new(success: true, message: message, errorCode: null, repositoryReason: null, pullRequestId: pullRequestId);

    public static PullRequestOperationResult Failure(
        string message,
        string errorCode,
        string repositoryReason,
        long? pullRequestId = null) =>
        new(success: false, message: message, errorCode: errorCode, repositoryReason: repositoryReason, pullRequestId: pullRequestId);
}
