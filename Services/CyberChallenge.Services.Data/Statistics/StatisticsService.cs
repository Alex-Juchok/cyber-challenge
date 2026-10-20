namespace CyberChallenge.Services.Data.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CyberChallenge.Data.Common.Repositories;
    using CyberChallenge.Data.Models;
    using CyberChallenge.Data.Models.Game;
    using CyberChallenge.Services.Data.Statistics.Models;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Получает агрегированную игровую статистику.
    /// Расчёты выполняются на стороне базы данных.
    /// </summary>
    public sealed class StatisticsService : IStatisticsService
    {
        private readonly IDeletableEntityRepository<GameAttempt> gameAttemptRepository;
        private readonly IDeletableEntityRepository<ApplicationUser> userRepository;

        public StatisticsService(
            IDeletableEntityRepository<GameAttempt> gameAttemptRepository,
            IDeletableEntityRepository<ApplicationUser> userRepository)
        {
            this.gameAttemptRepository = gameAttemptRepository;
            this.userRepository = userRepository;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<PlayerStatisticsDto>> GetLeaderboardAsync(
            int take = 100)
        {
            // Ограничиваем количество результатов разумным диапазоном.
            take = Math.Clamp(take, 1, 1000);

            // Группируем записи по игрокам и вычисляем агрегаты в SQL.
            var aggregates = await this.GetAggregates()
                .ToListAsync();

            if (aggregates.Count == 0)
            {
                return Array.Empty<PlayerStatisticsDto>();
            }

            // Получаем имена пользователей отдельным запросом.
            // Анонимные игровые попытки в таблицу лидеров не входят.
            var userIds = aggregates
                .Select(item => item.UserId)
                .ToArray();

            var users = await this.userRepository
                .AllAsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .Select(user => new
                {
                    user.Id,
                    user.UserName,
                })
                .ToDictionaryAsync(
                    user => user.Id,
                    user => user.UserName ?? "Игрок");

            // Сопоставляем агрегаты с именами пользователей.
            var players = aggregates
                .Where(item => users.ContainsKey(item.UserId))
                .Select(item => new PlayerStatisticsDto
                {
                    UserId = item.UserId,
                    UserName = users[item.UserId],
                    TotalScore = item.TotalScore,
                    CompletedLevels = item.CompletedLevels,
                    TotalAnswers = item.TotalAnswers,
                    FailedAnswers = item.FailedAnswers,
                    BestScore = item.BestScore,
                    AverageSuccessfulTimeSeconds =
                        item.AverageSuccessfulTimeSeconds,
                    LastPlayedOn = item.LastPlayedOn,
                })
                // Сначала очки, затем число пройденных уровней.
                // При равенстве учитываем среднее время прохождения.
                .OrderByDescending(player => player.TotalScore)
                .ThenByDescending(player => player.CompletedLevels)
                .ThenBy(player =>
                    player.AverageSuccessfulTimeSeconds ?? double.MaxValue)
                .Take(take)
                .ToList();

            // Присваиваем места после окончательной сортировки.
            for (var index = 0; index < players.Count; index++)
            {
                players[index].Rank = index + 1;
            }

            return players;
        }

        /// <inheritdoc />
        public async Task<PlayerStatisticsDto?> GetPlayerStatisticsAsync(
            string userId)
        {
            // Ищем агрегаты только для указанного игрока.
            var aggregate = await this.GetAggregates(userId)
                .FirstOrDefaultAsync();

            // Если у пользователя ещё нет игровых результатов,
            // статистика пока не сформирована.
            if (aggregate == null)
            {
                return null;
            }

            var user = await this.userRepository
                .AllAsNoTracking()
                .Where(item => item.Id == userId)
                .Select(item => new
                {
                    item.Id,
                    item.UserName,
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            return new PlayerStatisticsDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? "Игрок",
                TotalScore = aggregate.TotalScore,
                CompletedLevels = aggregate.CompletedLevels,
                TotalAnswers = aggregate.TotalAnswers,
                FailedAnswers = aggregate.FailedAnswers,
                BestScore = aggregate.BestScore,
                AverageSuccessfulTimeSeconds =
                    aggregate.AverageSuccessfulTimeSeconds,
                LastPlayedOn = aggregate.LastPlayedOn,
            };
        }

        /// <summary>
        /// Строит запрос агрегатов по всем игрокам
        /// либо по одному пользователю, если указан userId.
        /// </summary>
        private IQueryable<PlayerStatisticsAggregate> GetAggregates(
            string? userId = null)
        {
            var attempts = this.gameAttemptRepository
                .AllAsNoTracking()
                // Записи без пользователя не относятся к рейтингу игроков.
                .Where(attempt => attempt.UserId != null);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                attempts = attempts.Where(
                    attempt => attempt.UserId == userId);
            }

            return attempts
                .GroupBy(attempt => attempt.UserId!)
                .Select(group => new PlayerStatisticsAggregate
                {
                    UserId = group.Key,

                    // Суммируем очки всех записанных попыток.
                    TotalScore = group.Sum(attempt => attempt.Score),

                    // Каждая успешная запись соответствует
                    // успешному прохождению уровня.
                    CompletedLevels = group.Count(
                        attempt => attempt.IsSuccessful),

                    // Одна запись GameAttempt соответствует
                    // одной отправке ответа.
                    TotalAnswers = group.Count(),

                    // Неудачные отправки считаем по записям,
                    // а не суммируем FailedAttempts:
                    // это поле хранит накопительное значение сессии.
                    FailedAnswers = group.Count(
                        attempt => !attempt.IsSuccessful),

                    BestScore = group.Max(
                        attempt => attempt.Score),

                    // Среднее время считаем только по успешным
                    // прохождениям, исключая неудачные отправки.
                    AverageSuccessfulTimeSeconds = group
                        .Where(attempt => attempt.IsSuccessful)
                        .Select(attempt => (double?)attempt.ElapsedSeconds)
                        .Average(),

                    LastPlayedOn = group.Max(
                        attempt => attempt.PlayedOn),
                });
        }

        /// <summary>
        /// Внутренняя модель результата группировки в базе данных.
        /// </summary>
        private sealed class PlayerStatisticsAggregate
        {
            public string UserId { get; set; } = string.Empty;

            public int TotalScore { get; set; }

            public int CompletedLevels { get; set; }

            public int TotalAnswers { get; set; }

            public int FailedAnswers { get; set; }

            public int BestScore { get; set; }

            public double? AverageSuccessfulTimeSeconds { get; set; }

            public DateTime? LastPlayedOn { get; set; }
        }
    }
}