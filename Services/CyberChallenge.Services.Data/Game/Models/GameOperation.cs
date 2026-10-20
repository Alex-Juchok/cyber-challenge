
namespace CyberChallenge.Services.Data.Game.Models
{
    /// <summary>
    /// Арифметическая операция, используемая на игровом уровне.
    /// </summary>
    public enum GameOperation
    {
        /// <summary>
        /// Сложение выбранных чисел.
        /// </summary>
        Addition,

        /// <summary>
        /// Вычитание чисел в порядке их выбора.
        /// </summary>
        Subtraction,

        /// <summary>
        /// Умножение выбранных чисел.
        /// </summary>
        Multiplication,

        /// <summary>
        /// Комбинация нескольких арифметических операций.
        /// </summary>
        Combined,
    }
}
