namespace CyberChallenge.Web.ViewModels.Statistics
{
    using System.Collections.Generic;

    public sealed class StatisticsIndexViewModel
    {
        public IReadOnlyCollection<LeaderboardPlayerViewModel> Leaderboard { get; set; }
            = new List<LeaderboardPlayerViewModel>();

        public PlayerStatisticsViewModel? MyStatistics { get; set; }

        public bool IsAuthenticated { get; set; }

    }
}