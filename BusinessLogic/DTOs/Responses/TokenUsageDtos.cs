namespace BusinessLogic.DTOs.Responses;

public sealed record TokenUsageSummaryDto(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    int AnswerCount,
    DateTime? FirstUsedAt,
    DateTime? LastUsedAt);

public sealed record UserTokenUsageDto(
    int UserId,
    string FullName,
    string Email,
    string Role,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    int AnswerCount,
    DateTime? LastUsedAt);

public sealed record AdminTokenUsageDto(
    TokenUsageSummaryDto Summary,
    IReadOnlyList<UserTokenUsageDto> Users);
