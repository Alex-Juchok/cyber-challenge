namespace CyberChallenge.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using CyberChallenge.Data.Common.Repositories;
    using CyberChallenge.Data.Models.Game;
    using CyberChallenge.Services.Data.Game;
    using CyberChallenge.Services.Data.Game.Models;
    using CyberChallenge.Web.ViewModels.Game;

    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Обрабатывает игровые страницы и отправку решений игрока.
    /// </summary>
    public class GameController : BaseController
    {
        private readonly IGameService gameService;
        private readonly IDeletableEntityRepository<GameAttempt> gameAttemptRepository;

        /// <summary>
        /// Инициализирует контроллер необходимыми сервисами.
        /// </summary>
        public GameController(
            IGameService gameService,
            IDeletableEntityRepository<GameAttempt> gameAttemptRepository)
        {
            this.gameService = gameService;
            this.gameAttemptRepository = gameAttemptRepository;
        }

        /// <summary>
        /// Показывает выбранный уровень.
        /// GET: /Game?level=1
        /// </summary>
        public IActionResult Index(int level = 1)
        {
            var definition = this.gameService.GetLevel(level);

            if (definition == null)
            {
                return this.NotFound();
            }

            return this.View(this.CreateViewModel(definition));
        }

        /// <summary>
        /// Проверяет решение и сохраняет результат попытки.
        /// POST: /Game/Submit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(GameViewModel input)
        {
            var definition = this.gameService.GetLevel(input.LevelNumber);

            if (definition == null)
            {
                return this.NotFound();
            }

            // Проверяем только идентификаторы фишек.
            // Их числовые значения берутся из серверного описания уровня.
            var result = this.gameService.CheckSolution(
                input.LevelNumber,
                input.SelectedNumberIds);

            var model = this.CreateViewModel(definition);
            model.SelectedNumberIds = input.SelectedNumberIds ?? new();

            // Результат вычисления назначается сервером, а не браузером.
            model.CalculatedResult = result.CalculatedResult;
            model.FeedbackMessage = result.Message;
            model.IsLevelCompleted = result.IsCorrect;

            await this.SaveAttemptAsync(
                definition.LevelNumber,
                definition.TargetNumber,
                result);

            return this.View("Index", model);
        }

        /// <summary>
        /// Преобразует описание уровня в модель представления.
        /// </summary>
        private GameViewModel CreateViewModel(GameLevelDefinition definition)
        {
            return new GameViewModel
            {
                LevelNumber = definition.LevelNumber,
                TargetNumber = definition.TargetNumber,
                Title = definition.Title,
                Rules = definition.Rules,
                Numbers = definition.Numbers
                    .Select(number => new GameNumberViewModel
                    {
                        Id = number.Id,
                        Value = number.Value,
                    })
                    .ToList(),
            };
        }

        /// <summary>
        /// Сохраняет попытку для будущей страницы статистики.
        /// </summary>
        private async Task SaveAttemptAsync(
            int levelNumber,
            int targetNumber,
            GameSolutionResult result)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var attempt = new GameAttempt
            {
                UserId = userId,
                LevelNumber = levelNumber,
                TargetNumber = targetNumber,
                ResultNumber = result.CalculatedResult,
                IsSuccessful = result.IsCorrect,
                Score = result.IsCorrect ? 100 : 0,
                MovesCount = 0,
                ElapsedSeconds = 0,
                PlayedOn = DateTime.UtcNow,
            };

            await this.gameAttemptRepository.AddAsync(attempt);
            await this.gameAttemptRepository.SaveChangesAsync();
        }
    }
}