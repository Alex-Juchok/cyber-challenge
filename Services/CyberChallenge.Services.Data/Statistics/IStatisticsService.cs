namespace CyberChallenge.Services.Data.Statistics
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CyberChallenge.Services.Data.Statistics.Models;

    /// <summary>
    /// Определяет операции получения игровой статистики.
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Возвращает таблицу лидеров, отсортированную по очкам.
        /// </summary>
        Task<IReadOnlyCollection<PlayerStatisticsDto>> GetLeaderboardAsync(
            int take = 100);

        /// <summary>
        /// Возвращает агрегированную статистику конкретного игрока.
        /// </summary>
        Task<PlayerStatisticsDto?> GetPlayerStatisticsAsync(string userId);
    }
}