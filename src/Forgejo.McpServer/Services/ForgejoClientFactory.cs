using System.Net.Http;
using MarkZither.Forgejo.ApiClient;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;

namespace Forgejo.McpServer.Services;

public static class ForgejoClientFactory {
    public static ForgejoClient Create(string baseUrl, string token, HttpClient? httpClient = null) {
        if (string.IsNullOrWhiteSpace(baseUrl)) {
            throw new ArgumentException("A Forgejo base URL is required.", nameof(baseUrl));
        }

        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("A Forgejo token is required.", nameof(token));
        }

        var normalizedBaseUrl = NormalizeBaseUrl(baseUrl);
        var uri = new Uri(normalizedBaseUrl);
        var authProvider = new ApiKeyAuthenticationProvider(
            "Authorization",
            $"Bearer {token.Trim()}",
            ApiKeyAuthenticationProvider.KeyLocation.Header,
            new[] { uri.Host });

        var client = httpClient ?? new HttpClient();
        var requestAdapter = new HttpClientRequestAdapter(
            authProvider,
            new JsonParseNodeFactory(),
            new JsonSerializationWriterFactory(),
            client,
            new ObservabilityOptions());

        requestAdapter.BaseUrl = normalizedBaseUrl.TrimEnd('/') + "/api/v1";
        return new ForgejoClient(requestAdapter);
    }

    private static string NormalizeBaseUrl(string baseUrl) {
        var trimmed = baseUrl.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var result)) {
            throw new ArgumentException("The Forgejo base URL must be an absolute URI.", nameof(baseUrl));
        }

        return result.ToString().TrimEnd('/');
    }
}
