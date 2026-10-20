namespace CyberChallenge.Web.ViewModels.Statistics
{
    public sealed class LeaderboardPlayerViewModel
    {
        public int Rank { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public int TotalScore { get; set; }

        public int CompletedLevels { get; set; }

        public int FailedAnswers { get; set; }

        public int BestScore { get; set; }

        public double? AverageSuccessfulTimeSeconds { get; set; }
    }
}