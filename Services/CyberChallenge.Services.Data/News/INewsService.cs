namespace CyberChallenge.Services.Data.News
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CyberChallenge.Data.Models.News;

    /// <summary>
    /// Описывает операции административного управления новостями.
    /// </summary>
    public interface INewsService
    {
        Task<IReadOnlyCollection<NewsArticle>> GetAllAsync();

        Task<NewsArticle?> GetByIdAsync(string id);

        Task CreateAsync(NewsArticle article);

        Task<bool> UpdateAsync(NewsArticle article);

        Task<bool> DeleteAsync(string id);

        Task<bool> IncrementViewsAsync(string id);
    }
}