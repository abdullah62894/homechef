using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using Microsoft.Extensions.Options;

namespace HomeChef.Application.Features.Reports;

/// <summary>
/// Minimal text abuse guard: rejects user-written text containing any
/// configured blocked word. Deliberately naive (normalized substring match);
/// the admin moderation queue remains the main line of defense.
/// </summary>
public sealed class ContentGuard
{
    private readonly string[] _blockedWords;

    public ContentGuard(IOptions<ModerationOptions> options)
    {
        _blockedWords = options.Value.BlockedWords
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => Normalize(w))
            .ToArray();
    }

    /// <throws><see cref="BusinessException"/> with CONTENT_BLOCKED when the text is rejected.</throws>
    public void EnsureAllowed(string text)
    {
        if (_blockedWords.Length == 0)
        {
            return;
        }

        var normalized = Normalize(text);
        foreach (var word in _blockedWords)
        {
            if (normalized.Contains(word, StringComparison.Ordinal))
            {
                throw new BusinessException(
                    ErrorCodes.ContentBlocked,
                    "This content violates our community rules and cannot be posted.");
            }
        }
    }

    private static string Normalize(string text)
    {
        return new string(text.Select(c => char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : ' ').ToArray());
    }
}
