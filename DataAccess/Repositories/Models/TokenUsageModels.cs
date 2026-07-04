namespace DataAccess.Repositories.Models;

public sealed record TokenUsageAggregate(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    int AnswerCount,
    DateTime? FirstUsedAt,
    DateTime? LastUsedAt);

public sealed record UserTokenUsageAggregate(
    int UserId,
    string FullName,
    string Email,
    string Role,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    int AnswerCount,
    DateTime? LastUsedAt);
