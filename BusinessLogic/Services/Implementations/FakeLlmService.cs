using System.Text;
using System.Text.RegularExpressions;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Infrastructure.Settings;
using BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services.Implementations;

public sealed class FakeLlmService : ILlmService
{
    private readonly int _responseDelayMilliseconds;
    private readonly string _modelName;

    public FakeLlmService(IConfiguration configuration)
    {
        var settings = LlmSettings.FromConfiguration(configuration);
        _modelName = settings.Model;
        _responseDelayMilliseconds = int.TryParse(configuration["Llm:Fake:ResponseDelay"], out var delay) && delay > 0
            ? delay
            : 0;
    }

    public string ModelName => _modelName;

    public async Task<LlmResponseDto> GenerateAnswerAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (_responseDelayMilliseconds > 0)
        {
            await Task.Delay(_responseDelayMilliseconds, cancellationToken);
        }

        var snippets = ExtractContextSnippets(prompt).ToList();
        if (snippets.Count == 0)
        {
            var notFoundAnswer = "Không tìm thấy thông tin này trong tài liệu đã tải lên.";
            return new LlmResponseDto(notFoundAnswer, EstimateTokens(prompt), EstimateTokens(notFoundAnswer));
        }

        var builder = new StringBuilder();
        builder.AppendLine("Theo tài liệu đã tải lên, hệ thống đang chạy ở chế độ AI giả lập để demo luồng RAG.");
        builder.AppendLine();
        builder.AppendLine("Nội dung liên quan được tìm thấy:");

        foreach (var snippet in snippets.Take(3))
        {
            builder.AppendLine($"- {snippet}");
        }

        var answer = builder.ToString().Trim();
        return new LlmResponseDto(answer, EstimateTokens(prompt), EstimateTokens(answer));
    }

    private static IEnumerable<string> ExtractContextSnippets(string prompt)
    {
        var matches = Regex.Matches(
            prompt,
            @"Nội dung:\s*(?<content>.*?)(?=\r?\n\r?\n(?:\[Nguồn|Quy tắc trả lời:)|\z)",
            RegexOptions.Singleline | RegexOptions.CultureInvariant);

        foreach (Match match in matches)
        {
            var content = NormalizeWhitespace(match.Groups["content"].Value);
            if (content.Length == 0)
            {
                continue;
            }

            yield return content.Length <= 220 ? content : $"{content[..220]}...";
        }
    }

    private static string NormalizeWhitespace(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }

    private static int EstimateTokens(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling(value.Length / 4.0));
    }
}
