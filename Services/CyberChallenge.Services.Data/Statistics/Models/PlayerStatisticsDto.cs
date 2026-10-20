namespace CyberChallenge.Services.Data.Statistics.Models
{
    using System;

    /// <summary>
    /// Агрегированная статистика одного игрока.
    /// Используется для таблицы лидеров и личной статистики.
    /// </summary>
    public sealed class PlayerStatisticsDto
    {
        /// <summary>
        /// Идентификатор игрока в ASP.NET Core Identity.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Место игрока в таблице лидеров.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Общее количество набранных очков.
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// Количество успешно пройденных уровней.
        /// </summary>
        public int CompletedLevels { get; set; }

        /// <summary>
        /// Общее количество отправленных ответов.
        /// </summary>
        public int TotalAnswers { get; set; }

        /// <summary>
        /// Количество ошибочных ответов.
        /// </summary>
        public int FailedAnswers { get; set; }

        /// <summary>
        /// Наибольшее количество очков за одну попытку.
        /// </summary>
        public int BestScore { get; set; }

        /// <summary>
        /// Среднее время успешного прохождения уровня в секундах.
        /// Null, если игрок ещё не прошёл ни одного уровня.
        /// </summary>
        public double? AverageSuccessfulTimeSeconds { get; set; }

        /// <summary>
        /// Дата последней игровой активности в UTC.
        /// </summary>
        public DateTime? LastPlayedOn { get; set; }
    }
}