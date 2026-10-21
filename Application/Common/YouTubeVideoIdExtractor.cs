using System.Text.RegularExpressions;

namespace Application.Common;

public static partial class YouTubeVideoIdExtractor
{
    [GeneratedRegex(@"^[A-Za-z0-9_\-]{11}$")]
    private static partial Regex BareVideoIdRegex();

    [GeneratedRegex(
        @"^(?:https?:\/\/)?(?:www\.|m\.)?(?:youtube\.com\/watch\?(?:[^#]*&)?v=|youtube\.com\/(?:embed|shorts|live|v)\/|youtu\.be\/)([A-Za-z0-9_\-]{11})(?:[?&#].*)?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex YouTubeUrlRegex();

    public static bool TryExtract(string? input, out string videoId)
    {
        videoId = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var candidate = input.Trim();

        if (BareVideoIdRegex().IsMatch(candidate))
        {
            videoId = candidate;
            return true;
        }

        var match = YouTubeUrlRegex().Match(candidate);

        if (!match.Success)
            return false;

        videoId = match.Groups[1].Value;
        return true;
    }

    public static bool IsValidVideoId(string? videoId)
    {
        return !string.IsNullOrWhiteSpace(videoId) &&
               BareVideoIdRegex().IsMatch(videoId.Trim());
    }
}
