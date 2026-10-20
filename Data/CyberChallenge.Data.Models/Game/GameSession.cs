namespace CyberChallenge.Data.Models.Game
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using CyberChallenge.Data.Common.Models;

    /// <summary>
    /// Хранит состояние текущей игровой попытки.
    /// </summary>
    public class GameSession : BaseDeletableModel<Guid>
    {
        /// <summary>
        /// Создаёт игровую сессию с уникальным идентификатором.
        /// </summary>
        public GameSession()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Идентификатор игрока. Для незарегистрированного игрока равен null.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Номер уровня, который проходит игрок.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int LevelNumber { get; set; }

        /// <summary>
        /// Время начала попытки в UTC.
        /// </summary>
        public DateTime StartedOn { get; set; }

        /// <summary>
        /// Число неудачных отправок решения.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int FailedAttempts { get; set; }

        /// <summary>
        /// Признак завершения текущей попытки.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// JSON-представление уровня на момент начала попытки.
        /// Позволяет проверять ответ по той же раскладке фишек.
        /// </summary>
        [Required]
        public string LevelSnapshotJson { get; set; } = string.Empty;
    }
}
