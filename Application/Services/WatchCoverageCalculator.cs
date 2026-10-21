namespace Application.Services;

public static class WatchCoverageCalculator
{
    public sealed record Interval(double Start, double End);

    public static double MergeAndCalculateCoverageSeconds(
        IEnumerable<Interval> intervals,
        double durationSeconds)
    {
        if (durationSeconds <= 0)
            return 0;

        var ordered = intervals
            .Where(i => i.End > i.Start)
            .Select(i => new Interval(
                Math.Max(0, i.Start),
                Math.Min(i.End, durationSeconds)))
            .Where(i => i.End > i.Start)
            .OrderBy(i => i.Start)
            .ToList();

        if (ordered.Count == 0)
            return 0;

        double coverage = 0;
        var currentStart = ordered[0].Start;
        var currentEnd = ordered[0].End;

        for (var i = 1; i < ordered.Count; i++)
        {
            var segment = ordered[i];

            if (segment.Start <= currentEnd)
            {
                currentEnd = Math.Max(currentEnd, segment.End);
            }
            else
            {
                coverage += currentEnd - currentStart;
                currentStart = segment.Start;
                currentEnd = segment.End;
            }
        }

        coverage += currentEnd - currentStart;

        return Math.Min(coverage, durationSeconds);
    }

    public static decimal CalculateWatchedPercentage(
        IEnumerable<Interval> intervals,
        double durationSeconds)
    {
        if (durationSeconds <= 0)
            return 0m;

        var coverage = MergeAndCalculateCoverageSeconds(
            intervals,
            durationSeconds);

        var percentage = (decimal)(coverage / durationSeconds) * 100m;

        return Math.Min(100m, Math.Round(percentage, 2));
    }
}
