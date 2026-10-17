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
            string rules)
        {
            this.LevelNumber = levelNumber;
            this.TargetNumber = targetNumber;
            this.Numbers = numbers;
            this.Title = title;
            this.Rules = rules;
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
    }
}