namespace CyberChallenge.Web.Controllers
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using CyberChallenge.Services.Data.Statistics;
    using CyberChallenge.Web.ViewModels.Statistics;
    using Microsoft.AspNetCore.Mvc;

    public class StatisticsController : BaseController
    {
        private readonly IStatisticsService statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            this.statisticsService = statisticsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Получаем таблицу лидеров.
            var leaderboard = await this.statisticsService.GetLeaderboardAsync(100);

            var isAuthenticated = this.User.Identity?.IsAuthenticated == true;
            var userId = isAuthenticated
                ? this.User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            // Формируем основную модель страницы.
            var model = new StatisticsIndexViewModel
            {
                IsAuthenticated = isAuthenticated,
                Leaderboard = leaderboard
                    .Select(player => new LeaderboardPlayerViewModel
                    {
                        Rank = player.Rank,
                        UserId = player.UserId,
                        UserName = player.UserName,
                        TotalScore = player.TotalScore,
                        CompletedLevels = player.CompletedLevels,
                        FailedAnswers = player.FailedAnswers,
                        BestScore = player.BestScore,
                        AverageSuccessfulTimeSeconds =
                            player.AverageSuccessfulTimeSeconds,
                    })
                    .ToList(),
            };

            // Личную статистику показываем только авторизованному пользователю.
            if (!string.IsNullOrEmpty(userId))
            {
                var playerStatistics =
                    await this.statisticsService.GetPlayerStatisticsAsync(userId);

                if (playerStatistics != null)
                {
                    var playerRank = leaderboard
                        .FirstOrDefault(player => player.UserId == userId)?.Rank;

                    model.MyStatistics = new PlayerStatisticsViewModel
                    {
                        UserName = playerStatistics.UserName,
                        TotalScore = playerStatistics.TotalScore,
                        CompletedLevels = playerStatistics.CompletedLevels,
                        TotalAnswers = playerStatistics.TotalAnswers,
                        FailedAnswers = playerStatistics.FailedAnswers,
                        BestScore = playerStatistics.BestScore,
                        AverageSuccessfulTimeSeconds =
                            playerStatistics.AverageSuccessfulTimeSeconds,
                        LastPlayedOn = playerStatistics.LastPlayedOn,
                        Rank = playerRank,
                    };
                }
            }

            return this.View(model);
        }
    }
}