
namespace CyberChallenge.Services.Data.News
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CyberChallenge.Data.Models.News;

    public interface INewsService
    {
        Task<IReadOnlyCollection<NewsArticle>> GetAllAsync();

        Task<NewsArticle?> GetByIdAsync(int id);

        Task CreateAsync(NewsArticle article);

        Task<bool> UpdateAsync(NewsArticle article);

        Task<bool> DeleteAsync(int id);

        Task<bool> IncrementViewsAsync(int id);
    }
}
