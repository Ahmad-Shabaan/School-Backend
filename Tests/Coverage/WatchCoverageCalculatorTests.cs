using Application.Services;
using Xunit;

namespace Tests.Coverage;

public class WatchCoverageCalculatorTests
{
    [Fact]
    public void OverlappingIntervals_DoNotDoubleCount()
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(0, 10),
            new WatchCoverageCalculator.Interval(5, 15)
        };

        var coverage =
            WatchCoverageCalculator.MergeAndCalculateCoverageSeconds(
                intervals,
                durationSeconds: 100);

        Assert.Equal(15, coverage, precision: 5);
    }

    [Fact]
    public void AdjacentIntervals_AreMerged()
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(0, 10),
            new WatchCoverageCalculator.Interval(10, 20)
        };

        var coverage =
            WatchCoverageCalculator.MergeAndCalculateCoverageSeconds(
                intervals,
                durationSeconds: 100);

        Assert.Equal(20, coverage, precision: 5);
    }

    [Fact]
    public void UnorderedIntervals_AreCalculatedCorrectly()
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(50, 60),
            new WatchCoverageCalculator.Interval(0, 20),
            new WatchCoverageCalculator.Interval(15, 25)
        };

        var coverage =
            WatchCoverageCalculator.MergeAndCalculateCoverageSeconds(
                intervals,
                durationSeconds: 100);

        Assert.Equal(35, coverage, precision: 5);
    }

    [Fact]
    public void IntervalsWithinLargerInterval_DoNotIncreaseCoverage()
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(0, 50),
            new WatchCoverageCalculator.Interval(10, 20),
            new WatchCoverageCalculator.Interval(30, 40)
        };

        var coverage =
            WatchCoverageCalculator.MergeAndCalculateCoverageSeconds(
                intervals,
                durationSeconds: 100);

        Assert.Equal(50, coverage, precision: 5);
    }

    [Fact]
    public void OutOfRangeIntervals_AreClampedToDuration()
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(-10, 5),
            new WatchCoverageCalculator.Interval(95, 150)
        };

        var coverage =
            WatchCoverageCalculator.MergeAndCalculateCoverageSeconds(
                intervals,
                durationSeconds: 100);

        Assert.Equal(10, coverage, precision: 5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidDuration_ReturnsZero(double duration)
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(0, 10)
        };

        var coverage =
            WatchCoverageCalculator.MergeAndCalculateCoverageSeconds(
                intervals,
                duration);

        Assert.Equal(0, coverage, precision: 5);
    }

    [Fact]
    public void EmptyIntervals_ReturnZeroCoverage()
    {
        var coverage =
            WatchCoverageCalculator.MergeAndCalculateCoverageSeconds(
                Array.Empty<WatchCoverageCalculator.Interval>(),
                durationSeconds: 100);

        Assert.Equal(0, coverage, precision: 5);
    }

    [Fact]
    public void FullCoverage_ReportsOneHundredPercent()
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(0, 40),
            new WatchCoverageCalculator.Interval(35, 100)
        };

        var percentage =
            WatchCoverageCalculator.CalculateWatchedPercentage(
                intervals,
                durationSeconds: 100);

        Assert.Equal(100m, percentage);
    }

    [Fact]
    public void PartialCoverage_ReportsExpectedPercentage()
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(0, 45),
            new WatchCoverageCalculator.Interval(50, 70)
        };

        var percentage =
            WatchCoverageCalculator.CalculateWatchedPercentage(
                intervals,
                durationSeconds: 100);

        Assert.Equal(65m, percentage);
    }

    [Fact]
    public void ShortSeekAtTheEnd_DoesNotReachNinetyPercent()
    {
        var intervals = new[]
        {
            new WatchCoverageCalculator.Interval(95, 100)
        };

        var percentage =
            WatchCoverageCalculator.CalculateWatchedPercentage(
                intervals,
                durationSeconds: 100);

        Assert.Equal(5m, percentage);
        Assert.True(percentage < 90m);
    }
}
