using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;
using DataAccess.Repositories.Models;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementations;

public sealed class TokenUsageService : ITokenUsageService
{
    private readonly IChatRepository _chatRepository;
    private readonly ILogger<TokenUsageService> _logger;

    public TokenUsageService(
        IChatRepository chatRepository,
        ILogger<TokenUsageService> logger)
    {
        _chatRepository = chatRepository;
        _logger = logger;
    }

    public async Task<TokenUsageSummaryDto> GetUserUsageAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return MapSummary(await _chatRepository.GetTokenUsageByUserAsync(userId, cancellationToken));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load token usage for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AdminTokenUsageDto> GetAdminUsageAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _chatRepository.GetTokenUsageAsync(cancellationToken);
            var users = await _chatRepository.GetTokenUsageByUsersAsync(cancellationToken);

            return new AdminTokenUsageDto(
                MapSummary(summary),
                users.Select(MapUser).ToList());
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load admin token usage");
            throw;
        }
    }

    private static TokenUsageSummaryDto MapSummary(TokenUsageAggregate usage)
    {
        return new TokenUsageSummaryDto(
            usage.PromptTokens,
            usage.CompletionTokens,
            usage.TotalTokens,
            usage.AnswerCount,
            usage.FirstUsedAt,
            usage.LastUsedAt);
    }

    private static UserTokenUsageDto MapUser(UserTokenUsageAggregate usage)
    {
        return new UserTokenUsageDto(
            usage.UserId,
            usage.FullName,
            usage.Email,
            usage.Role,
            usage.PromptTokens,
            usage.CompletionTokens,
            usage.TotalTokens,
            usage.AnswerCount,
            usage.LastUsedAt);
    }
}
