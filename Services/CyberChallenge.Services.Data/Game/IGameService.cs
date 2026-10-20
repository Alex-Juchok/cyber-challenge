namespace CyberChallenge.Services.Data.Game
{
    using System.Collections.Generic;

    using CyberChallenge.Services.Data.Game.Models;

    /// <summary>
    /// Определяет операции игровой логики.
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Возвращает описание указанного уровня.
        /// </summary>
        GameLevelDefinition? GetLevel(int levelNumber);

        /// <summary>
        /// Проверяет выбранные фишки по переданному описанию уровня.
        /// </summary>
        GameSolutionResult CheckSolution(
            GameLevelDefinition level,
            IReadOnlyCollection<string> selectedNumberIds);
    }
}