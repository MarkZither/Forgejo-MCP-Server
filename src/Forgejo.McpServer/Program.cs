using System.Security.Cryptography;
using System.Text;
using Forgejo.McpServer.Configuration;
using Forgejo.McpServer.Services;
using Forgejo.McpServer.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

static string? GetArgumentValue(string[] args, string optionName) {
    for (var i = 0; i < args.Length; i++) {
        var arg = args[i];
        if (arg.Equals("--" + optionName, StringComparison.OrdinalIgnoreCase)) {
            if (i + 1 < args.Length) {
                return args[i + 1];
            }

            return null;
        }

        if (arg.StartsWith("--" + optionName + "=", StringComparison.OrdinalIgnoreCase)) {
            return arg[("--" + optionName + "=").Length..];
        }
    }

    return null;
}

static string ResolveLogDirectory() {
    var overridePath = Environment.GetEnvironmentVariable("FORGEJO_LOG_PATH");
    if (!string.IsNullOrWhiteSpace(overridePath)) {
        Directory.CreateDirectory(overridePath);
        return overridePath;
    }

    var repoLogs = Path.Combine(Environment.CurrentDirectory, "logs");
    Directory.CreateDirectory(repoLogs);
    return repoLogs;
}

static string RedactSecret(string? value) {
    if (string.IsNullOrWhiteSpace(value)) {
        return "[empty]";
    }

    var digest = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    var fingerprint = Convert.ToHexString(digest).Substring(0, 12).ToLowerInvariant();
    return $"len={value.Length}; sha256={fingerprint}";
}

var repoRoot = Environment.CurrentDirectory;
var logDirectory = ResolveLogDirectory();
var logPath = Path.Combine(logDirectory, "forgejo-mcp-.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try {
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddOptions<ForgejoOptions>()
        .BindConfiguration("Forgejo");

    var forgejoOptions = builder.Configuration.GetSection("Forgejo").Get<ForgejoOptions>() ?? new ForgejoOptions();
    var currentDirectory = Environment.CurrentDirectory;
    var baseUrl = GetArgumentValue(args, "forgejo-base-url")
        ?? forgejoOptions.BaseUrl
        ?? throw new InvalidOperationException("Forgejo:BaseUrl is required. Set it in user secrets or with the environment variable Forgejo__BaseUrl.");
    var token = GetArgumentValue(args, "forgejo-token")
        ?? forgejoOptions.Token
        ?? throw new InvalidOperationException("Forgejo:Token is required. Set it in user secrets or with the environment variable Forgejo__Token.");

    Log.Information("Forgejo MCP server starting");
    Log.Information("CurrentDirectory={CurrentDirectory}", currentDirectory);
    Log.Information("ResolvedRepoRoot={RepoRoot}", repoRoot);
    Log.Information("LogDirectory={LogDirectory}", logDirectory);
    Log.Information("LogFile={LogFile}", logPath);
    Log.Information("ForgejoBaseUrl={BaseUrl}", baseUrl);
    Log.Information("ForgejoToken={Token}", RedactSecret(token));

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger, dispose: true);

    // Keep stdout reserved for MCP protocol traffic; route console logs to stderr.
    builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

    builder.Services.AddSingleton(ForgejoClientFactory.Create(baseUrl, token));
    builder.Services.AddSingleton<ForgejoPullRequestService>();

    // Add the MCP services: the transport to use (stdio) and the tools to register.
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithTools<ForgejoPullRequestTools>();

    await builder.Build().RunAsync();
} finally {
    Log.CloseAndFlush();
}
