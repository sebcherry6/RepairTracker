using RepairTracker.Domain;

namespace RepairTracker.Tests;

public class TenancyTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);

    [Theory]
    [InlineData("2025-12-31", false)]   // day before it starts
    [InlineData("2026-01-01", true)]    // first day
    [InlineData("2030-06-01", true)]    // years later, still ongoing
    public void Ongoing_tenancy_is_current_from_its_start(string day, bool expected)
    {
        var tenancy = new Tenancy { StartDate = Start };

        Assert.Equal(expected, tenancy.IsCurrent(DateOnly.Parse(day)));
    }

    [Fact]
    public void Last_day_still_counts_and_the_next_does_not()
    {
        var tenancy = new Tenancy { StartDate = Start };
        tenancy.End(new DateOnly(2026, 6, 30));

        Assert.True(tenancy.IsCurrent(new DateOnly(2026, 6, 30)));
        Assert.False(tenancy.IsCurrent(new DateOnly(2026, 7, 1)));
    }

    [Fact]
    public void Cannot_end_before_it_starts()
    {
        var tenancy = new Tenancy { StartDate = Start };

        Assert.Throws<ArgumentException>(() => tenancy.End(new DateOnly(2025, 12, 1)));
    }
}