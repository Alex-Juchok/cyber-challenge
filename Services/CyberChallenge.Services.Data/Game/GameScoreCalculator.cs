using System;

namespace CyberChallenge.Services.Data.Game
{
    /// <summary>
    /// Рассчитывает награду за успешно пройденный уровень.
    /// </summary>
    public sealed class GameScoreCalculator
    {
        private const int BaseScore = 100;
        private const int MaximumScore = 200;

        /// <summary>
        /// Рассчитывает очки за успешное прохождение уровня.
        /// </summary>
        /// <param name="isSuccessful">Пройден ли уровень.</param>
        /// <param name="totalChips">Количество доступных фишек.</param>
        /// <param name="usedChips">Количество использованных фишек.</param>
        /// <param name="elapsedSeconds">Время прохождения в секундах.</param>
        /// <param name="failedAttempts">Количество неудачных попыток.</param>
        /// <param name="timeLimitSeconds">Лимит времени, если он задан.</param>
        /// <returns>Количество начисленных очков.</returns>
        public int Calculate(
            bool isSuccessful,
            int totalChips,
            int usedChips,
            int elapsedSeconds,
            int failedAttempts,
            int? timeLimitSeconds)
        {
            // За неудачное прохождение очки не начисляются.
            if (!isSuccessful)
            {
                return 0;
            }

            // Меньше использованных фишек — выше бонус.
            int chipBonus = Math.Max(0, totalChips - usedChips) * 3;

            // Бонус за скорость начисляется только в пределах лимита.
            int timeBonus = timeLimitSeconds.HasValue
                ? Math.Max(
                    0,
                    timeLimitSeconds.Value - elapsedSeconds)
                : Math.Max(0, 30 - elapsedSeconds);

            // Каждая ошибка уменьшает бонус, но не базовую награду.
            int attemptBonus = Math.Max(0, 20 - failedAttempts * 5);

            return Math.Min(
                MaximumScore,
                BaseScore + chipBonus + timeBonus + attemptBonus);
        }
    }
}
