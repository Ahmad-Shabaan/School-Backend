namespace Application.Common;

public class LearningPlatformOptions
{
    public const string SectionName = "LearningPlatform";

    public decimal VideoCompletionThresholdPercent { get; set; } = 90m;

    public decimal QuizPassingThresholdPercent { get; set; } = 80m;

    public double MaxVideoDurationSeconds { get; set; } = 43200;

    public double MaxWatchSegmentSeconds { get; set; } = 120;

    public double ProgressPositionToleranceSeconds { get; set; } = 5;

    public int MaxCommentsPageSize { get; set; } = 20;
}
