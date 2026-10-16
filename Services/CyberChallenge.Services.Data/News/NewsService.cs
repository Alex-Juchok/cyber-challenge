
namespace CyberChallenge.Services.Data.News
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CyberChallenge.Data.Common.Repositories;
    using CyberChallenge.Data.Models.News;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Управляет новостными публикациями.
    /// </summary>
    public class NewsService : INewsService
    {
        private readonly IDeletableEntityRepository<NewsArticle> repository;

        public NewsService(
            IDeletableEntityRepository<NewsArticle> repository)
        {
            this.repository = repository;
        }

        public async Task<IReadOnlyCollection<NewsArticle>> GetAllAsync()
        {
            return await this.repository
                .AllAsNoTracking()
                .OrderByDescending(article => article.PublishedOn)
                .ToListAsync();
        }

        public async Task<NewsArticle?> GetByIdAsync(int id)
        {
            return await this.repository
                .AllAsNoTracking()
                .FirstOrDefaultAsync(article => article.Id == id);
        }

        public async Task CreateAsync(NewsArticle article)
        {
            await this.repository.AddAsync(article);
            await this.repository.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(NewsArticle article)
        {
            var existing = await this.repository
                .All()
                .FirstOrDefaultAsync(item => item.Id == article.Id);

            if (existing == null)
            {
                return false;
            }

            existing.Title = article.Title;
            existing.Content = article.Content;
            existing.ImagePath = article.ImagePath;
            existing.PublishedOn = article.PublishedOn;

            await this.repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var article = await this.repository
                .All()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (article == null)
            {
                return false;
            }

            // Репозиторий помечает сущность удалённой,
            // не удаляя запись физически из базы.
            this.repository.Delete(article);
            await this.repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IncrementViewsAsync(int id)
        {
            // Нужна отслеживаемая сущность, так как изменяется счётчик.
            var article = await this.repository
                .All()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (article == null)
            {
                return false;
            }

            article.ViewsCount++;
            await this.repository.SaveChangesAsync();

            return true;
        }
    }
}
