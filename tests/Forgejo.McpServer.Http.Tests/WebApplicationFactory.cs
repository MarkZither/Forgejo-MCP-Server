using Microsoft.AspNetCore.Mvc.Testing;
using TUnit.Core.Interfaces;

namespace Forgejo.McpServer.http.tests;

public class WebApplicationFactory : WebApplicationFactory<Program>, IAsyncInitializer {
    public Task InitializeAsync() {
        _ = Server;

        return Task.CompletedTask;
    }
}
