using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Presentation.Models;

namespace Presentation.Pages.AdminTokenUsage;

[Authorize(Roles = "Admin")]
public sealed class IndexModel : AppPageModel
{
    private readonly ITokenUsageService _tokenUsageService;

    public IndexModel(ITokenUsageService tokenUsageService)
    {
        _tokenUsageService = tokenUsageService;
    }

    public AdminTokenUsageViewModel ViewModel { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var usage = await _tokenUsageService.GetAdminUsageAsync(cancellationToken);
        ViewModel = new AdminTokenUsageViewModel
        {
            Summary = new TokenUsageSummaryViewModel
            {
                PromptTokens = usage.Summary.PromptTokens,
                CompletionTokens = usage.Summary.CompletionTokens,
                TotalTokens = usage.Summary.TotalTokens,
                AnswerCount = usage.Summary.AnswerCount,
                FirstUsedAt = usage.Summary.FirstUsedAt,
                LastUsedAt = usage.Summary.LastUsedAt
            },
            Users = usage.Users.Select(user => new UserTokenUsageViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PromptTokens = user.PromptTokens,
                CompletionTokens = user.CompletionTokens,
                TotalTokens = user.TotalTokens,
                AnswerCount = user.AnswerCount,
                LastUsedAt = user.LastUsedAt
            }).ToList()
        };
    }
}
