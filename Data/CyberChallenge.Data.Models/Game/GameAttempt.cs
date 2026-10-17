namespace CyberChallenge.Data.Models.Game
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using CyberChallenge.Data.Common.Models;

    /// <summary>
    /// Хранит результат одной попытки прохождения игрового уровня.
    /// Эти записи будут использоваться для построения статистики игрока.
    /// </summary>
    public class GameAttempt : BaseDeletableModel<Guid>
    {
        public GameAttempt()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Идентификатор пользователя из ASP.NET Core Identity.
        /// Null позволяет сохранить результат и для неавторизованного игрока.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Номер уровня, на котором была совершена попытка.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int LevelNumber { get; set; }

        /// <summary>
        /// Число, которое игрок должен был получить.
        /// </summary>
        public int TargetNumber { get; set; }

        /// <summary>
        /// Результат вычисления, отправленный игроком.
        /// </summary>
        public int ResultNumber { get; set; }

        /// <summary>
        /// Количество использованных фишек в данной попытке.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int MovesCount { get; set; }

        /// <summary>
        /// Время, затраченное на попытку, в секундах.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int ElapsedSeconds { get; set; }

        /// <summary>
        /// Признак успешного решения задания.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Количество очков, полученных за попытку.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Score { get; set; }

        /// <summary>
        /// Дата и время завершения попытки в UTC.
        /// </summary>
        public DateTime PlayedOn { get; set; } = DateTime.UtcNow;
    }
}