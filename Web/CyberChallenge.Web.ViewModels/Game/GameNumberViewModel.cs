namespace CyberChallenge.Web.ViewModels.Game
{
    /// <summary>
    /// Данные одной числовой фишки для отображения в браузере.
    /// </summary>
    public sealed class GameNumberViewModel
    {
        public string Id { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}