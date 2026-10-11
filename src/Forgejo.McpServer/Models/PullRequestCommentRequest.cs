using MarkZither.Forgejo.ApiClient.Models;

namespace Forgejo.McpServer.Models;

public sealed class PullRequestCommentRequest {
    public PullRequestCommentRequest(
        string owner,
        string repository,
        long pullRequestNumber,
        string body) {
        Owner = owner;
        Repository = repository;
        PullRequestNumber = pullRequestNumber;
        Body = body;
    }

    public string Owner { get; }
    public string Repository { get; }
    public long PullRequestNumber { get; }
    public string Body { get; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Owner) &&
        !string.IsNullOrWhiteSpace(Repository) &&
        PullRequestNumber > 0 &&
        !string.IsNullOrWhiteSpace(Body);

    public CreateIssueCommentOption ToCreateIssueCommentOption() {
        if (!IsValid) {
            throw new InvalidOperationException("The pull request comment request is not valid for submission.");
        }

        return new CreateIssueCommentOption {
            Body = Body.Trim(),
        };
    }
}
