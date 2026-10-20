namespace CyberChallenge.Web.ViewModels.Game
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using CyberChallenge.Services.Data.Game.Models;

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


        /// <summary>
        /// Идентификатор серверной игровой сессии.
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// Ограничение времени уровня в секундах.
        /// Null означает, что ограничение не задано.
        /// </summary>
        [BindNever]
        public int? TimeLimitSeconds { get; set; }

        /// <summary>
        /// Количество секунд, прошедших с начала попытки.
        /// Используется для отображения таймера.
        /// </summary>
        [BindNever]
        public int ElapsedSeconds { get; set; }

        /// <summary>
        /// Количество неудачных попыток в текущей сессии.
        /// </summary>
        [BindNever]
        public int FailedAttempts { get; set; }

        /// <summary>
        /// Операция, которую нужно использовать для получения целевого числа.
        /// </summary>
        [BindNever]
        public GameOperation Operation { get; set; }

        /// <summary>
        /// Идентификатор обязательной фишки, если она задана для уровня.
        /// </summary>
        [BindNever]
        public string? RequiredNumberId { get; set; }

        /// <summary>
        /// Признак того, что попытка завершена и продолжать её нельзя.
        /// </summary>
        [BindNever]
        public bool IsAttemptFinished { get; set; }

    }
}