using Application.Common;
using Xunit;

namespace Tests.YouTube;

public class YouTubeVideoIdExtractorTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("http://youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ&t=42s&list=PL123", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?list=PL123&v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=10", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("  dQw4w9WgXcQ  ", "dQw4w9WgXcQ")]
    public void SupportedForms_ExtractVideoId(string input, string expected)
    {
        var success = YouTubeVideoIdExtractor.TryExtract(input, out var videoId);

        Assert.True(success);
        Assert.Equal(expected, videoId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("short")]
    [InlineData("dQw4w9WgXcQtoolong11")]
    [InlineData("https://example.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch")]
    [InlineData("https://www.youtube.com/watch?v=short")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://evil-youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("ftp://youtube.com/watch?v=dQw4w9WgXcQ")]
    public void InvalidInput_IsRejected(string? input)
    {
        var success = YouTubeVideoIdExtractor.TryExtract(input, out var videoId);

        Assert.False(success);
        Assert.Equal(string.Empty, videoId);
    }

    [Theory]
    [InlineData("dQw4w9WgXcQ", true)]
    [InlineData("abc-def_123", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidVideoId_ChecksFormat(string? videoId, bool expected)
    {
        Assert.Equal(expected, YouTubeVideoIdExtractor.IsValidVideoId(videoId));
    }

    [Fact]
    public void QueryStringContainingBadValues_DoesNotExtract()
    {
        var success = YouTubeVideoIdExtractor.TryExtract(
            "https://www.youtube.com/watch?v=<script>",
            out _);

        Assert.False(success);
    }
}
