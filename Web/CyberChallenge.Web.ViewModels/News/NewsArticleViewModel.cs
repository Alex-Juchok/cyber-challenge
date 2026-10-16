
namespace CyberChallenge.Web.ViewModels.News
{
    using System;

    /// <summary>
    /// Данные новости для отображения пользователю.
    /// </summary>
    public class NewsArticleViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public int ViewsCount { get; set; }

        public DateTime PublishedOn { get; set; }
    }
}
