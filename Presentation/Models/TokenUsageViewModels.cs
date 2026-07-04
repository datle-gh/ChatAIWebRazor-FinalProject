namespace Presentation.Models;

public sealed class TokenUsageSummaryViewModel
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public int AnswerCount { get; set; }
    public DateTime? FirstUsedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public sealed class UserTokenUsageViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public int AnswerCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public sealed class AdminTokenUsageViewModel
{
    public TokenUsageSummaryViewModel Summary { get; set; } = new();
    public List<UserTokenUsageViewModel> Users { get; set; } = new();
}
