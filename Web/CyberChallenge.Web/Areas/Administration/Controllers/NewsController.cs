namespace CyberChallenge.Web.Areas.Administration.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using CyberChallenge.Data.Models.News;
    using CyberChallenge.Services.Data.News;
    using CyberChallenge.Web.ViewModels.News;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public class NewsController : AdministrationController
    {
        private const long MaxImageSize = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly INewsService newsService;
        private readonly IWebHostEnvironment environment;

        public NewsController(
            INewsService newsService,
            IWebHostEnvironment environment)
        {
            this.newsService = newsService;
            this.environment = environment;
        }

        /// <summary>
        /// Отображает панель с новостями.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var articles = await this.newsService.GetAllAsync();
            return this.View(articles);
        }

        /// <summary>
        /// Показывает форму создания новости.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new NewsArticleInputModel());
        }

        /// <summary>
        /// Создаёт новость.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsArticleInputModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            string? imagePath = null;

            if (model.Image != null)
            {
                imagePath = await this.SaveImageAsync(model.Image);

                if (imagePath == null)
                {
                    return this.View(model);
                }
            }

            var article = new NewsArticle
            {
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                ImagePath = imagePath,
                PublishedOn = DateTime.UtcNow,
            };

            await this.newsService.CreateAsync(article);

            this.TempData["SuccessMessage"] = "Новость создана.";
            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        /// Показывает форму редактирования новости.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var article = await this.newsService.GetByIdAsync(id);

            if (article == null)
            {
                return this.NotFound();
            }

            return this.View(new NewsArticleInputModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                ExistingImagePath = article.ImagePath,
            });
        }

        /// <summary>
        /// Сохраняет изменения новости.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NewsArticleInputModel model)
        {
            var article = await this.newsService.GetByIdAsync(model.Id);

            if (article == null)
            {
                return this.NotFound();
            }

            model.ExistingImagePath = article.ImagePath;

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var oldImagePath = article.ImagePath;
            var newImagePath = oldImagePath;

            if (model.Image != null)
            {
                newImagePath = await this.SaveImageAsync(model.Image);

                if (newImagePath == null)
                {
                    return this.View(model);
                }
            }
            else if (model.RemoveImage)
            {
                newImagePath = null;
            }

            article.Title = model.Title.Trim();
            article.Content = model.Content.Trim();
            article.ImagePath = newImagePath;

            var updated = await this.newsService.UpdateAsync(article);

            if (!updated)
            {
                if (oldImagePath != newImagePath)
                {
                    this.DeleteImage(newImagePath);
                }

                return this.NotFound();
            }

            // Старый файл удаляем только после успешного сохранения.
            if (oldImagePath != newImagePath)
            {
                this.DeleteImage(oldImagePath);
            }

            this.TempData["SuccessMessage"] = "Новость обновлена.";
            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        /// Удаляет новость.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var article = await this.newsService.GetByIdAsync(id);

            if (article == null)
            {
                return this.NotFound();
            }

            var deleted = await this.newsService.DeleteAsync(id);

            if (deleted)
            {
                this.DeleteImage(article.ImagePath);
                this.TempData["SuccessMessage"] = "Новость удалена.";
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        /// Сохраняет допустимое изображение в wwwroot/uploads/news.
        /// Возвращает относительный URL либо null при ошибке проверки.
        /// </summary>
        private async Task<string?> SaveImageAsync(IFormFile image)
        {
            if (image.Length == 0 || image.Length > MaxImageSize)
            {
                this.ModelState.AddModelError(
                    nameof(NewsArticleInputModel.Image),
                    "Фотография должна быть размером не более 5 МБ.");
                return null;
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (Array.IndexOf(AllowedExtensions, extension) < 0)
            {
                this.ModelState.AddModelError(
                    nameof(NewsArticleInputModel.Image),
                    "Разрешены форматы JPG, JPEG, PNG и WEBP.");
                return null;
            }

            var webRoot = this.environment.WebRootPath
                ?? Path.Combine(this.environment.ContentRootPath, "wwwroot");

            var uploadDirectory = Path.Combine(webRoot, "uploads", "news");
            Directory.CreateDirectory(uploadDirectory);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadDirectory, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
            {
                await image.CopyToAsync(stream);
            }

            return $"/uploads/news/{fileName}";
        }

        /// <summary>
        /// Удаляет только файлы из каталога загрузок новостей.
        /// </summary>
        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)
                || !imagePath.StartsWith(
                    "/uploads/news/",
                    StringComparison.Ordinal))
            {
                return;
            }

            var fileName = Path.GetFileName(imagePath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var webRoot = this.environment.WebRootPath
                ?? Path.Combine(this.environment.ContentRootPath, "wwwroot");

            var fullPath = Path.Combine(
                webRoot, "uploads", "news", fileName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}