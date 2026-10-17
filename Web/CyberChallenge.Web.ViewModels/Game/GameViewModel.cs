namespace CyberChallenge.Web.ViewModels.Game
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// Содержит данные игрового экрана и результат проверки решения.
    /// </summary>
    public sealed class GameViewModel
    {
        /// <summary>
        /// Номер отображаемого уровня.
        /// </summary>
        public int LevelNumber { get; set; }

        /// <summary>
        /// Число, которое необходимо получить.
        /// </summary>
        public int TargetNumber { get; set; }

        /// <summary>
        /// Название текущего уровня.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Правила текущего уровня.
        /// </summary>
        public string Rules { get; set; } = string.Empty;

        /// <summary>
        /// Фишки, доступные для перетаскивания.
        /// </summary>
        public IReadOnlyCollection<GameNumberViewModel> Numbers { get; set; }
            = new List<GameNumberViewModel>();

        /// <summary>
        /// Идентификаторы фишек, которые игрок выбрал для решения.
        /// </summary>
        public List<string> SelectedNumberIds { get; set; } = new();

        /// <summary>
        /// Сумма, рассчитанная сервером после проверки.
        /// </summary>
        [BindNever]
        public int? CalculatedResult { get; set; }

        /// <summary>
        /// Сообщение об успешном решении или ошибке.
        /// </summary>
        [BindNever]
        public string? FeedbackMessage { get; set; }

        /// <summary>
        /// Признак успешного прохождения уровня.
        /// </summary>
        [BindNever]
        public bool IsLevelCompleted { get; set; }
    }
}