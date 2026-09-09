using MarkZither.Forgejo.ApiClient.Models;

namespace Forgejo.McpServer.Models;

public sealed class PullRequestCreateRequest {
    public PullRequestCreateRequest(
        string owner,
        string repository,
        string headBranch,
        string baseBranch,
        string title,
        string? body = null) {
        Owner = owner;
        Repository = repository;
        HeadBranch = headBranch;
        BaseBranch = baseBranch;
        Title = title;
        Body = body;
    }

    public string Owner { get; }
    public string Repository { get; }
    public string HeadBranch { get; }
    public string BaseBranch { get; }
    public string Title { get; }
    public string? Body { get; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Owner) &&
        !string.IsNullOrWhiteSpace(Repository) &&
        !string.IsNullOrWhiteSpace(HeadBranch) &&
        !string.IsNullOrWhiteSpace(BaseBranch) &&
        !string.IsNullOrWhiteSpace(Title) &&
        !string.Equals(HeadBranch.Trim(), BaseBranch.Trim(), StringComparison.OrdinalIgnoreCase);

    public CreatePullRequestOption ToCreatePullRequestOption() {
        if (!IsValid) {
            throw new InvalidOperationException("The pull request request is not valid for submission.");
        }

        return new CreatePullRequestOption {
            Base = BaseBranch.Trim(),
            Head = HeadBranch.Trim(),
            Title = Title.Trim(),
            Body = string.IsNullOrWhiteSpace(Body) ? null : Body.Trim(),
        };
    }
}
