namespace CyberChallenge.Web.ViewModels.News
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Данные формы создания и редактирования новости.
    /// </summary>
    public class NewsArticleInputModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Введите заголовок.")]
        [StringLength(200)]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите текст новости.")]
        [StringLength(20000)]
        [Display(Name = "Текст новости")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Фотография")]
        public IFormFile? Image { get; set; }

        // Путь к уже загруженной фотографии при редактировании.
        public string? ExistingImagePath { get; set; }

        // Позволяет явно убрать старую фотографию.
        [Display(Name = "Удалить фотографию")]
        public bool RemoveImage { get; set; }
    }
}
