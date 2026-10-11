using System.Net;
using System.Net.Http;
using System.Text;
using Forgejo.McpServer.Configuration;
using Forgejo.McpServer.Models;
using Forgejo.McpServer.Services;
using MarkZither.Forgejo.ApiClient.Models;
using Microsoft.Extensions.Configuration;

namespace Forgejo.McpServer.stdio.tests;

public class ForgejoPullRequestFoundationTests {
    private static bool HasRealIntegrationConfiguration() {
        var baseUrl = ForgejoTestSettings.BaseUrl;
        var token = ForgejoTestSettings.Token;

        return !string.IsNullOrWhiteSpace(baseUrl)
            && !string.IsNullOrWhiteSpace(token)
            && !string.Equals(baseUrl, "https://forgejo.home", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(baseUrl, "https://forgejo.example.com", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(token, "temptoken", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(token, "secret-token", StringComparison.OrdinalIgnoreCase);
    }

    [Test]
    [Category("Unit")]
    public async Task PullRequestCreateRequest_RejectsMissingTitle() {
        var request = new PullRequestCreateRequest(
            "acme",
            "demo-repo",
            "feature/test",
            "main",
            " ");

        await Assert.That(request.IsValid).IsFalse();
    }

    [Test]
    [Category("Unit")]
    [Arguments("feature/test", "main", true)]
    [Arguments("release-2026.09", "develop", true)]
    [Arguments("bugfix/issue-123", "main", true)]
    [Arguments("feature invalid", "main", false)]
    [Arguments("feature@{oops}", "main", false)]
    [Arguments("feature..bad", "main", false)]
    [Arguments(".bad", "main", false)]
    [Arguments("main", "main", false)]
    public async Task PullRequestCreateRequest_ValidatesBranchPairs(string headBranch, string baseBranch, bool expectedIsValid) {
        var request = new PullRequestCreateRequest(
            "acme",
            "demo-repo",
            headBranch,
            baseBranch,
            "PR with branch validation");

        await Assert.That(request.IsValid).IsEqualTo(expectedIsValid);
    }

    [Test]
    [Category("Integration")]
    public async Task ForgejoPullRequestService_Validation_CreatePullRequest_UsesConfiguredRepo() {
        if (!HasRealIntegrationConfiguration()) {
            return;
        }

        var baseUrl = ForgejoTestSettings.BaseUrl ?? throw new InvalidOperationException("Forgejo base URL is required for integration testing.");
        var token = ForgejoTestSettings.Token ?? throw new InvalidOperationException("Forgejo token is required for integration testing.");
        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(baseUrl, token));
        var request = new PullRequestCreateRequest(
            "mark",
            "MCPTest",
            "tomerge",
            "main",
            "Integration validation PR",
            "Generated for end-to-end validation of the Forgejo MCP create PR flow.");

        var result = await service.CreateAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.PullRequestId).IsNotNull();
    }

    [Test]
    [Category("Integration")]
    public async Task ForgejoPullRequestService_Validation_RejectsInvalidBranchNames() {
        if (!HasRealIntegrationConfiguration()) {
            return;
        }

        var baseUrl = ForgejoTestSettings.BaseUrl ?? throw new InvalidOperationException("Forgejo base URL is required for integration testing.");
        var token = ForgejoTestSettings.Token ?? throw new InvalidOperationException("Forgejo token is required for integration testing.");
        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(baseUrl, token));
        var request = new PullRequestCreateRequest(
            "mark",
            "MCPTest",
            "feature invalid",
            "main",
            "This should be rejected before any repository call");

        var result = await service.CreateAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("validation_error");
    }

    [Test]
    public async Task ForgejoOptions_BindsStandardForgejoConfigurationKeys() {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["Forgejo:BaseUrl"] = "https://forgejo.example.com",
                ["Forgejo:Token"] = "secret-token"
            })
            .Build();

        var options = configuration.GetSection("Forgejo").Get<ForgejoOptions>();

        await Assert.That(options).IsNotNull();
        await Assert.That(options!.BaseUrl).IsEqualTo("https://forgejo.example.com");
        await Assert.That(options.Token).IsEqualTo("secret-token");
    }

    [Test]
    public async Task PullRequestOperationResult_ReportsRepositoryFailure() {
        var result = PullRequestOperationResult.Failure(
            "Branch protection blocks merge.",
            "branch_protection",
            "Required review not satisfied.");

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("branch_protection");
        await Assert.That(result.RepositoryReason).IsEqualTo("Required review not satisfied.");
    }

    [Test]
    public async Task ForgejoPullRequestService_RejectsSameBranchPullRequest() {
        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create("https://forgejo.example.com", "secret-token"));
        var request = new PullRequestCreateRequest(
            "acme",
            "demo-repo",
            "main",
            "main",
            "Same branch pull request");

        var result = await service.CreateAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("validation_error");
        await Assert.That(result.RepositoryReason).Contains("source branch");
    }

    [Test]
    public async Task ForgejoClientFactory_UsesProvidedBaseUrlAndToken() {
        var client = ForgejoClientFactory.Create("https://forgejo.example.com", "secret-token");

        await Assert.That(client).IsNotNull();
    }

    [Test]
    public async Task ForgejoPullRequestService_FailsWhenCreateResponseIsNotVisibleInRepo() {
        using var server = new LocalForgejoPullRequestServer();
        server.Start();

        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(server.BaseUrl, "secret-token", server.HttpClient));
        var request = new PullRequestCreateRequest(
            "mark",
            "demo-repo",
            "feature/visible-check",
            "main",
            "Visible PR validation",
            "This PR is returned from the create endpoint but not visible in the repo.");

        var result = await service.CreateAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("pull_request_not_visible");
    }

    [Test]
    public async Task ForgejoPullRequestService_AddComment_RecordsDiscussionComment() {
        using var server = new LocalPullRequestDiscussionServer();
        server.Start();

        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(server.BaseUrl, "secret-token", server.HttpClient));
        var request = new PullRequestCommentRequest(
            "mark",
            "demo-repo",
            42,
            "Thanks for the update. The latest notes are clear.");

        var result = await service.AddCommentAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Message).Contains("Comment");
    }

    [Test]
    public async Task ForgejoPullRequestService_GetDiscussion_ReturnsLatestStateAndComments() {
        using var server = new LocalPullRequestDiscussionServer();
        server.Start();

        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(server.BaseUrl, "secret-token", server.HttpClient));

        var discussion = await service.GetDiscussionAsync("mark", "demo-repo", 42, CancellationToken.None);

        await Assert.That(discussion.Success).IsTrue();
        await Assert.That(discussion.PullRequestState).IsEqualTo("open");
        await Assert.That(discussion.Comments.Count).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task ForgejoPullRequestService_CommentingMissingPullRequest_ReturnsNotFound() {
        using var server = new LocalPullRequestDiscussionServer();
        server.Start();

        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(server.BaseUrl, "secret-token", server.HttpClient));
        var request = new PullRequestCommentRequest("mark", "demo-repo", 999, "This should fail");

        var result = await service.AddCommentAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("not_found");
    }

    [Test]
    public async Task PullRequestMergeRequest_ValidatesMergeStrategy() {
        var request = new PullRequestMergeRequest(
            "acme",
            "demo-repo",
            42,
            "squash",
            "Ship the fix");

        await Assert.That(request.IsValid).IsTrue();
        var option = request.ToMergePullRequestOption();
        await Assert.That(option.Do).IsEqualTo(MergePullRequestOption_Do.Squash);
        await Assert.That(option.MergeMessageField).IsEqualTo("Ship the fix");
    }

    [Test]
    public async Task ForgejoPullRequestService_Merge_MergesSuccessfulPullRequest() {
        using var server = new LocalMergePullRequestServer();
        server.Start();

        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(server.BaseUrl, "secret-token", server.HttpClient));
        var request = new PullRequestMergeRequest(
            "mark",
            "demo-repo",
            42,
            "merge",
            "Merged by the Forgejo MCP workflow");

        var result = await service.MergeAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Merged).IsTrue();
        await Assert.That(result.Message).Contains("merged");
    }

    [Test]
    public async Task ForgejoPullRequestService_Merge_RejectsBlockedPullRequest() {
        using var server = new LocalMergePullRequestServer();
        server.Start();

        var service = new ForgejoPullRequestService(ForgejoClientFactory.Create(server.BaseUrl, "secret-token", server.HttpClient));
        var request = new PullRequestMergeRequest(
            "mark",
            "demo-repo",
            99,
            "merge",
            "This should not merge");

        var result = await service.MergeAsync(request, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.ErrorCode).IsEqualTo("merge_blocked");
    }

    private sealed class LocalMergePullRequestServer : IDisposable {
        private readonly HttpClient _httpClient;

        public string BaseUrl { get; } = "https://forgejo.localtest.me";
        public HttpClient HttpClient => _httpClient;

        public LocalMergePullRequestServer() {
            _httpClient = new HttpClient(new StubHttpMessageHandler(request => {
                var path = request.RequestUri?.AbsolutePath ?? string.Empty;

                if (request.Method == HttpMethod.Get && path == "/api/v1/repos/mark/demo-repo/pulls/42") {
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent("{\"id\":42,\"number\":42,\"state\":\"open\",\"merged\":false,\"title\":\"Ready for merge\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Get && path == "/api/v1/repos/mark/demo-repo/pulls/99") {
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent("{\"id\":99,\"number\":99,\"state\":\"open\",\"merged\":false,\"title\":\"Blocked merge\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Post && path == "/api/v1/repos/mark/demo-repo/pulls/42/merge") {
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent("{\"merged\":true,\"message\":\"Pull Request successfully merged\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Post && path == "/api/v1/repos/mark/demo-repo/pulls/99/merge") {
                    return new HttpResponseMessage(HttpStatusCode.Conflict) {
                        Content = new StringContent("{\"message\":\"Merge blocked by required branch protection checks\"}", Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound) {
                    Content = new StringContent("{\"message\":\"Not Found\"}", Encoding.UTF8, "application/json")
                };
            }));
        }

        public void Start() {
        }

        public void Dispose() {
            _httpClient.Dispose();
        }
    }

    private sealed class LocalPullRequestDiscussionServer : IDisposable {
        private readonly HttpClient _httpClient;

        public string BaseUrl { get; } = "https://forgejo.localtest.me";
        public HttpClient HttpClient => _httpClient;

        public LocalPullRequestDiscussionServer() {
            _httpClient = new HttpClient(new StubHttpMessageHandler(request => {
                var path = request.RequestUri?.AbsolutePath ?? string.Empty;

                if (request.Method == HttpMethod.Get && path == "/api/v1/repos/mark/demo-repo/pulls/42") {
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent("{\"id\":42,\"number\":42,\"title\":\"Ready for review\",\"state\":\"open\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Get && path == "/api/v1/repos/mark/demo-repo/issues/42/comments") {
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent("[{\"id\":1,\"body\":\"Initial review note\"},{\"id\":2,\"body\":\"Looks good\"}]", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Post && path == "/api/v1/repos/mark/demo-repo/issues/42/comments") {
                    return new HttpResponseMessage(HttpStatusCode.Created) {
                        Content = new StringContent("{\"id\":101,\"body\":\"Thanks for the update. The latest notes are clear.\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Post && path == "/api/v1/repos/mark/demo-repo/issues/999/comments") {
                    return new HttpResponseMessage(HttpStatusCode.NotFound) {
                        Content = new StringContent("{\"message\":\"Not Found\"}", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Get && path == "/api/v1/repos/mark/demo-repo/pulls/999") {
                    return new HttpResponseMessage(HttpStatusCode.NotFound) {
                        Content = new StringContent("{\"message\":\"Not Found\"}", Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound) {
                    Content = new StringContent("{\"message\":\"Not Found\"}", Encoding.UTF8, "application/json")
                };
            }));
        }

        public void Start() {
        }

        public void Dispose() {
            _httpClient.Dispose();
        }
    }

    private sealed class LocalForgejoPullRequestServer : IDisposable {
        private readonly HttpClient _httpClient;

        public string BaseUrl { get; } = "https://forgejo.localtest.me";
        public HttpClient HttpClient => _httpClient;

        public LocalForgejoPullRequestServer() {
            _httpClient = new HttpClient(new StubHttpMessageHandler(request => {
                var path = request.RequestUri?.AbsolutePath ?? string.Empty;

                if (request.Method == HttpMethod.Post && path == "/api/v1/repos/mark/demo-repo/pulls") {
                    return new HttpResponseMessage(HttpStatusCode.Created) {
                        Content = new StringContent("{\"id\":42,\"number\":42,\"title\":\"Visible PR validation\",\"state\":\"open\",\"head\":{\"ref\":\"feature/visible-check\"},\"base\":{\"ref\":\"main\"}}", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Get && path == "/api/v1/repos/mark/demo-repo/pulls") {
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent("[]", Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound) {
                    Content = new StringContent("{\"message\":\"Not Found\"}", Encoding.UTF8, "application/json")
                };
            }));
        }

        public void Start() {
        }

        public void Dispose() {
            _httpClient.Dispose();
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) {
            _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            return Task.FromResult(_responseFactory(request));
        }
    }
}



