using Forgejo.McpServer.Configuration;
using Microsoft.Extensions.Configuration;

internal sealed class ForgejoTestSecretsMarker;

internal static class ForgejoTestSettings {
    private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddUserSecrets<ForgejoTestSecretsMarker>(optional: true)
        .Build();

    private static ForgejoOptions Options =>
        _configuration.GetSection("Forgejo").Get<ForgejoOptions>() ?? new ForgejoOptions();

    public static string? BaseUrl => Options.BaseUrl;

    public static string? Token => Options.Token;
}
