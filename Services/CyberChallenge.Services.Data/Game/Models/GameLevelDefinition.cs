namespace CyberChallenge.Services.Data.Game.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Содержит неизменяемое описание игрового уровня.
    /// </summary>
    public sealed class GameLevelDefinition
    {
        /// <summary>
        /// Создаёт описание уровня.
        /// </summary>
        public GameLevelDefinition(
            int levelNumber,
            int targetNumber,
            IReadOnlyCollection<GameNumberDefinition> numbers,
            string title,
            string rules,
            GameOperation operation,
            int? timeLimitSeconds,
            string? requiredNumberId)
        {
            this.LevelNumber = levelNumber;
            this.TargetNumber = targetNumber;
            this.Numbers = numbers;
            this.Title = title;
            this.Rules = rules;
            this.Operation = operation;
            this.TimeLimitSeconds = timeLimitSeconds;
            this.RequiredNumberId = requiredNumberId;
        }

        /// <summary>
        /// Порядковый номер уровня.
        /// </summary>
        public int LevelNumber { get; }

        /// <summary>
        /// Число, которое требуется получить.
        /// </summary>
        public int TargetNumber { get; }

        /// <summary>
        /// Доступные игроку фишки.
        /// </summary>
        public IReadOnlyCollection<GameNumberDefinition> Numbers { get; }

        /// <summary>
        /// Название уровня.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Правила, отображаемые в интерфейсе.
        /// </summary>
        public string Rules { get; }

        /// <summary>
        /// Арифметическая операция текущего уровня.
        /// </summary>
        public GameOperation Operation { get; }

        /// <summary>
        /// Ограничение времени в секундах. Null означает отсутствие ограничения.
        /// </summary>
        public int? TimeLimitSeconds { get; }

        /// <summary>
        /// Идентификатор фишки, обязательной для использования.
        /// Null означает, что обязательной фишки нет.
        /// </summary>
        public string? RequiredNumberId { get; }
    }
}