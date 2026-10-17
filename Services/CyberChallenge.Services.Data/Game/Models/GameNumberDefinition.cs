namespace CyberChallenge.Services.Data.Game.Models
{
    /// <summary>
    /// Описывает число, доступное игроку на конкретном уровне.
    /// </summary>
    public sealed class GameNumberDefinition
    {
        /// <summary>
        /// Создаёт описание игровой фишки.
        /// </summary>
        /// <param name="id">Уникальный в пределах уровня идентификатор фишки.</param>
        /// <param name="value">Числовое значение фишки.</param>
        public GameNumberDefinition(string id, int value)
        {
            this.Id = id;
            this.Value = value;
        }

        /// <summary>
        /// Идентификатор фишки, используемый при отправке решения.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Значение, отображаемое внутри круга.
        /// </summary>
        public int Value { get; }
    }
}