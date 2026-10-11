using MarkZither.Forgejo.ApiClient.Models;

namespace Forgejo.McpServer.Models;

public sealed class PullRequestMergeRequest {
    public PullRequestMergeRequest(
        string owner,
        string repository,
        long pullRequestNumber,
        string mergeStrategy,
        string? mergeMessage = null) {
        Owner = owner;
        Repository = repository;
        PullRequestNumber = pullRequestNumber;
        MergeStrategy = mergeStrategy;
        MergeMessage = mergeMessage;
    }

    public string Owner { get; }
    public string Repository { get; }
    public long PullRequestNumber { get; }
    public string MergeStrategy { get; }
    public string? MergeMessage { get; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Owner) &&
        !string.IsNullOrWhiteSpace(Repository) &&
        PullRequestNumber > 0 &&
        !string.IsNullOrWhiteSpace(MergeStrategy) &&
        IsSupportedMergeStrategy(MergeStrategy);

    public MergePullRequestOption ToMergePullRequestOption() {
        if (!IsValid) {
            throw new InvalidOperationException("The pull request merge request is not valid for submission.");
        }

        return new MergePullRequestOption {
            Do = ParseMergeStrategy(MergeStrategy),
            MergeMessageField = string.IsNullOrWhiteSpace(MergeMessage) ? null : MergeMessage.Trim(),
            MergeTitleField = string.IsNullOrWhiteSpace(MergeMessage) ? null : MergeMessage.Trim(),
        };
    }

    private static bool IsSupportedMergeStrategy(string strategy) {
        var normalized = strategy.Trim();

        return normalized.Equals("merge", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("rebase", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("rebase-merge", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("squash", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("fast-forward-only", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("manually-merged", StringComparison.OrdinalIgnoreCase);
    }

    private static MergePullRequestOption_Do ParseMergeStrategy(string strategy) {
        var normalized = strategy.Trim();

        return normalized.ToLowerInvariant() switch {
            "merge" => MergePullRequestOption_Do.Merge,
            "rebase" => MergePullRequestOption_Do.Rebase,
            "rebase-merge" => MergePullRequestOption_Do.RebaseMerge,
            "squash" => MergePullRequestOption_Do.Squash,
            "fast-forward-only" => MergePullRequestOption_Do.FastForwardOnly,
            "manually-merged" => MergePullRequestOption_Do.ManuallyMerged,
            _ => MergePullRequestOption_Do.Merge,
        };
    }
}
