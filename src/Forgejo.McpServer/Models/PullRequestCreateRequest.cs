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
        IsValidBranchName(HeadBranch) &&
        IsValidBranchName(BaseBranch) &&
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

    private static bool IsValidBranchName(string? branch) {
        if (string.IsNullOrWhiteSpace(branch)) {
            return false;
        }

        var trimmed = branch.Trim();
        if (trimmed.Length > 255 || trimmed.Length == 0) {
            return false;
        }

        if (trimmed.StartsWith('/') || trimmed.EndsWith('/') || trimmed.StartsWith('.') || trimmed.EndsWith('.') || trimmed.Contains("//") || trimmed.Contains("..")) {
            return false;
        }

        if (trimmed.EndsWith(".lock", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("@{") || trimmed.Contains(" ") || char.IsWhiteSpace(trimmed[0])) {
            return false;
        }

        foreach (var character in trimmed) {
            if (char.IsWhiteSpace(character)) {
                return false;
            }

            if (character is '~' or '^' or ':' or '?' or '*' or '[' or '\\' or '@') {
                return false;
            }

            if (!char.IsLetterOrDigit(character) && character is not '-' and not '_' and not '/' and not '.') {
                return false;
            }
        }

        return true;
    }
}
