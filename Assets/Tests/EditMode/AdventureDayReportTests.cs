using NUnit.Framework;

public sealed class AdventureDayReportTests
{
    [Test]
    public void DawnMood_PerfectNight_AllowsGreedierPlans()
    {
        string line = AdventureDayReport.GetDawnMoodLine(0, 0, 0, true, true);

        StringAssert.Contains("Perfect night", line);
    }

    [Test]
    public void DawnMood_CleanThreatsWithoutDemeter_WarnsVoyageIsSlipping()
    {
        string line = AdventureDayReport.GetDawnMoodLine(0, 0, 0, false, true);

        StringAssert.Contains("voyage is slipping", line.ToLowerInvariant());
    }

    [Test]
    public void DawnMood_MessyNight_StartsDayTwoUnderPressure()
    {
        string line = AdventureDayReport.GetDawnMoodLine(1, 2, 1, false, false);

        StringAssert.Contains("pressure", line.ToLowerInvariant());
    }
}
