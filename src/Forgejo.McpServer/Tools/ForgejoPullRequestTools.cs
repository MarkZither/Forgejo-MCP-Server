using System.ComponentModel;
using Forgejo.McpServer.Models;
using Forgejo.McpServer.Services;
using ModelContextProtocol.Server;

namespace Forgejo.McpServer.Tools;

public sealed class ForgejoPullRequestTools {
    private readonly ForgejoPullRequestService _service;

    public ForgejoPullRequestTools(ForgejoPullRequestService service) {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [McpServerTool]
    [Description("Creates a pull request in a Forgejo repository from a valid source and target branch pair.")]
    public async Task<PullRequestOperationResult> CreatePullRequest(
        [Description("Forgejo owner or organization name.")] string owner,
        [Description("Repository name containing the change.")] string repository,
        [Description("Source branch that contains the work to be proposed.")] string headBranch,
        [Description("Target branch that should receive the change.")] string baseBranch,
        [Description("Pull request title.")] string title,
        [Description("Optional pull request description or summary.")] string? body = null,
        CancellationToken cancellationToken = default) {
        var request = new PullRequestCreateRequest(owner, repository, headBranch, baseBranch, title, body);
        return await _service.CreateAsync(request, cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool]
    [Description("Adds a comment to the discussion thread for an existing Forgejo pull request.")]
    public async Task<PullRequestOperationResult> AddPullRequestComment(
        [Description("Forgejo owner or organization name.")] string owner,
        [Description("Repository name containing the pull request.")] string repository,
        [Description("Pull request number to comment on.")] long pullRequestNumber,
        [Description("Comment content to append to the PR discussion.")] string body,
        CancellationToken cancellationToken = default) {
        var request = new PullRequestCommentRequest(owner, repository, pullRequestNumber, body);
        return await _service.AddCommentAsync(request, cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool]
    [Description("Retrieves the current pull request state and the latest visible discussion comments for a Forgejo pull request.")]
    public async Task<PullRequestDiscussionResult> GetPullRequestDiscussion(
        [Description("Forgejo owner or organization name.")] string owner,
        [Description("Repository name containing the pull request.")] string repository,
        [Description("Pull request number whose discussion should be fetched.")] long pullRequestNumber,
        CancellationToken cancellationToken = default) {
        return await _service.GetDiscussionAsync(owner, repository, pullRequestNumber, cancellationToken).ConfigureAwait(false);
    }
}
