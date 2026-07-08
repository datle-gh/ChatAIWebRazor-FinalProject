using BusinessLogic.Services.Interfaces;
using System.Text.Json;
using BusinessLogic.DTOs.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.Implementations;

public sealed class SystemSettingsService : ISystemSettingsService
{
    private readonly string _filePath;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemSettingsService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SystemSettingsService(
        IOptions<SystemSettingsFilePathOptions> options,
        IConfiguration configuration,
        ILogger<SystemSettingsService> logger)
    {
        _filePath = options.Value.FilePath;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SystemSettingsDto> GetSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return CreateDefaultSettings();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            return NormalizeSettings(
                JsonSerializer.Deserialize<SystemSettingsDto>(json, JsonOptions)
                ?? new SystemSettingsDto());
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to read settings from {FilePath}", _filePath);
            return CreateDefaultSettings();
        }
    }

    public async Task SaveSettingsAsync(
        SystemSettingsDto settings,
        CancellationToken cancellationToken = default)
    {
        settings = NormalizeSettings(settings);
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);

        _logger.LogInformation("System settings saved to {FilePath}", _filePath);
    }



    private SystemSettingsDto CreateDefaultSettings()
    {
        var settings = new SystemSettingsDto
        {
            TopK = ReadInt("RagSettings:TopK", 5),
            SimilarityThreshold = ReadDecimal("RagSettings:SimilarityThreshold", 0.7m),
            MaxCitationSnippetLength = ReadInt("RagSettings:MaxCitationSnippetLength", 250),
            ChunkSizeMode = ReadString("RagSettings:ChunkSizeMode", "Page"),
            PageChunkSize = ReadInt("RagSettings:PageChunkSize", 1),
            WordChunkSize = ReadInt("RagSettings:WordChunkSize", ReadInt("RagSettings:MaxChunkTokens", 700)),
            CharacterChunkSize = ReadInt("RagSettings:CharacterChunkSize", 3000),
            ChunkOverlapSize = ReadInt("RagSettings:ChunkOverlapSize", ReadInt("RagSettings:ChunkOverlapTokens", 100)),
            MinChunkCharacters = ReadInt("RagSettings:MinChunkCharacters", 30)
        };

        return NormalizeSettings(settings);
    }

    private string ReadString(string key, string fallback)
    {
        var value = _configuration[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private int ReadInt(string key, int fallback)
    {
        return int.TryParse(_configuration[key], out var value) && value > 0
            ? value
            : fallback;
    }

    private decimal ReadDecimal(string key, decimal fallback)
    {
        return decimal.TryParse(_configuration[key], out var value)
            ? value
            : fallback;
    }

    private static SystemSettingsDto NormalizeSettings(SystemSettingsDto settings)
    {
        settings.ChunkSizeMode = NormalizeChunkSizeMode(settings.ChunkSizeMode);
        settings.PageChunkSize = Math.Clamp(settings.PageChunkSize, 1, 20);
        settings.WordChunkSize = Math.Clamp(settings.WordChunkSize, 50, 3000);
        settings.CharacterChunkSize = Math.Clamp(settings.CharacterChunkSize, 200, 20000);
        settings.ChunkOverlapSize = Math.Clamp(settings.ChunkOverlapSize, 0, 2000);
        settings.MinChunkCharacters = Math.Clamp(settings.MinChunkCharacters, 1, 1000);
        return settings;
    }

    private static string NormalizeChunkSizeMode(string? mode)
    {
        if (string.Equals(mode, "Page", StringComparison.OrdinalIgnoreCase))
        {
            return "Page";
        }

        if (string.Equals(mode, "Character", StringComparison.OrdinalIgnoreCase))
        {
            return "Character";
        }

        return "Word";
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(
        string provider,
        string apiKey,
        string model,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return (false, "Vui lòng nhập API Key.");
        }

        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}?key={apiKey}";
                var response = await httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kết nối Gemini API thành công!");
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, $"Lỗi Gemini API: {response.StatusCode}. {body}");
            }

            if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.openai.com/v1/models");
                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kết nối OpenAI API thành công!");
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, $"Lỗi OpenAI API: {response.StatusCode}. {body}");
            }

            return (false, $"Provider '{provider}' không được hỗ trợ kiểm tra kết nối.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Test connection failed for {Provider}", provider);
            return (false, $"Không thể kết nối: {exception.Message}");
        }
    }
}
