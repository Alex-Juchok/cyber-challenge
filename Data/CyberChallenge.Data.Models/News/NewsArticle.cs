
namespace CyberChallenge.Data.Models.News
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using CyberChallenge.Data.Common.Models;

    /// <summary>
    /// Новостная публикация.
    /// </summary>
    public class NewsArticle : BaseDeletableModel<string>
    {
        public NewsArticle()
        {
            // Генерируем UUID в виде строки при создании нового объекта.
            this.Id = Guid.NewGuid().ToString();
        }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(20000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Относительный путь к загруженному изображению.
        /// </summary>
        [StringLength(500)]
        public string? ImagePath { get; set; }

        /// <summary>
        /// Количество открытий страницы новости.
        /// </summary>
        public int ViewsCount { get; set; }

        /// <summary>
        /// Дата публикации в UTC.
        /// </summary>
        public DateTime PublishedOn { get; set; } = DateTime.UtcNow;
    }
}
