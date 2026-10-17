namespace CyberChallenge.Services.Data.Game.Models
{
    /// <summary>
    /// Результат проверки решения игрока.
    /// </summary>
    public sealed class GameSolutionResult
    {
        public GameSolutionResult(
            bool isCorrect,
            int calculatedResult,
            string message)
        {
            this.IsCorrect = isCorrect;
            this.CalculatedResult = calculatedResult;
            this.Message = message;
        }

        /// <summary>
        /// Показывает, совпал ли результат с целью уровня.
        /// </summary>
        public bool IsCorrect { get; }

        /// <summary>
        /// Сумма выбранных чисел, рассчитанная сервером.
        /// </summary>
        public int CalculatedResult { get; }

        /// <summary>
        /// Сообщение для игрока.
        /// </summary>
        public string Message { get; }
    }
}