
namespace CyberChallenge.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using CyberChallenge.Services.Data.News;
    using CyberChallenge.Web.ViewModels.News;
    using Microsoft.AspNetCore.Mvc;

    public class NewsController : BaseController
    {
        private readonly INewsService newsService;

        public NewsController(INewsService newsService)
        {
            this.newsService = newsService;
        }

        // GET: /News
        public async Task<IActionResult> Index()
        {
            var articles = await this.newsService.GetAllAsync();

            var model = articles.Select(article => new NewsArticleViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                ImagePath = article.ImagePath,
                ViewsCount = article.ViewsCount,
                PublishedOn = article.PublishedOn,
            }).ToList();

            return this.View(model);
        }

        // GET: /News/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Не показываем несуществующие или удалённые новости.
            var article = await this.newsService.GetByIdAsync(id);

            if (article == null)
            {
                return this.NotFound();
            }

            await this.newsService.IncrementViewsAsync(id);

            // Получаем актуальный счётчик после увеличения.
            article = await this.newsService.GetByIdAsync(id);

            if (article == null)
            {
                return this.NotFound();
            }

            var model = new NewsArticleViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                ImagePath = article.ImagePath,
                ViewsCount = article.ViewsCount,
                PublishedOn = article.PublishedOn,
            };

            return this.View(model);
        }
    }
}
