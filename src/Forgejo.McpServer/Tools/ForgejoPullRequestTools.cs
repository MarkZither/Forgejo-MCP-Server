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
}
