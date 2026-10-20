namespace CyberChallenge.Web.ViewModels.Statistics
{
    using System;

    public sealed class PlayerStatisticsViewModel
    {
        public string UserName { get; set; } = string.Empty;

        public int TotalScore { get; set; }

        public int CompletedLevels { get; set; }

        public int TotalAnswers { get; set; }

        public int FailedAnswers { get; set; }

        public int BestScore { get; set; }

        public double? AverageSuccessfulTimeSeconds { get; set; }

        public DateTime? LastPlayedOn { get; set; }

        public int? Rank { get; set; }
    }
}