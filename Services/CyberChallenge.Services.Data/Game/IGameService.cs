namespace CyberChallenge.Services.Data.Game
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CyberChallenge.Services.Data.Game.Models;

    /// <summary>
    /// Определяет операции игровой логики.
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Возвращает описание указанного уровня.
        /// </summary>
        Task<GameLevelDefinition?> GetLevelAsync(int levelNumber);

        /// <summary>
        /// Проверяет выбранные фишки по переданному описанию уровня.
        /// </summary>
        GameSolutionResult CheckSolution(
            GameLevelDefinition level,
            IReadOnlyCollection<string> selectedNumberIds);
    }
}