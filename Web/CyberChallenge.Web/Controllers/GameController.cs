
namespace CyberChallenge.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;

    using CyberChallenge.Data.Common.Repositories;
    using CyberChallenge.Data.Models.Game;
    using CyberChallenge.Services.Data.Game;
    using CyberChallenge.Services.Data.Game.Models;
    using CyberChallenge.Web.ViewModels.Game;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Обрабатывает игровые страницы и отправку решений игрока.
    /// </summary>
    public class GameController : BaseController
    {
        private readonly IGameService gameService;
        private readonly IDeletableEntityRepository<GameAttempt> gameAttemptRepository;
        private readonly IDeletableEntityRepository<GameSession> gameSessionRepository;
        private readonly GameScoreCalculator scoreCalculator;

        /// <summary>
        /// Инициализирует контроллер необходимыми сервисами и репозиториями.
        /// </summary>
        public GameController(
            IGameService gameService,
            IDeletableEntityRepository<GameAttempt> gameAttemptRepository,
            IDeletableEntityRepository<GameSession> gameSessionRepository,
            GameScoreCalculator scoreCalculator)
        {
            this.gameService = gameService;
            this.gameAttemptRepository = gameAttemptRepository;
            this.gameSessionRepository = gameSessionRepository;
            this.scoreCalculator = scoreCalculator;
        }

        /// <summary>
        /// Создаёт новую попытку прохождения уровня.
        /// GET: /Game?level=1
        /// </summary>
        public async Task<IActionResult> Index(int level = 1)
        {
            var definition = this.gameService.GetLevel(level);

            if (definition == null)
            {
                return this.NotFound();
            }

            // Сохраняем точную раскладку уровня для последующей проверки.
            var session = new GameSession
            {
                UserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier),
                LevelNumber = definition.LevelNumber,
                StartedOn = DateTime.UtcNow,
                FailedAttempts = 0,
                IsCompleted = false,
                LevelSnapshotJson = JsonSerializer.Serialize(definition),
            };

            await this.gameSessionRepository.AddAsync(session);
            await this.gameSessionRepository.SaveChangesAsync();

            return this.View(this.CreateViewModel(definition, session, 0));
        }

        /// <summary>
        /// Проверяет решение по сохранённой раскладке уровня.
        /// POST: /Game/Submit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(GameViewModel input)
        {
            // Получаем сессию из базы, а не доверяем номеру уровня из формы.
            var session = await this.gameSessionRepository
                .All()
                .FirstOrDefaultAsync(item => item.Id == input.SessionId);

            if (session == null)
            {
                return this.NotFound();
            }

            // Проверяем, что сессия принадлежит текущему пользователю.
            var currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (session.UserId != currentUserId)
            {
                return this.Forbid();
            }

            // Нельзя повторно получать награду за завершённую сессию.
            if (session.IsCompleted)
            {
                return this.BadRequest("Эта игровая попытка уже завершена.");
            }

            // Восстанавливаем именно ту раскладку, с которой началась попытка.
            GameLevelDefinition? definition;

            try
            {
                definition = JsonSerializer.Deserialize<GameLevelDefinition>(
                    session.LevelSnapshotJson);
            }
            catch (JsonException)
            {
                return this.StatusCode(500, "Не удалось загрузить игровую сессию.");
            }

            if (definition == null)
            {
                return this.StatusCode(500, "Игровая сессия содержит некорректный уровень.");
            }

            // Время рассчитывается сервером, а не передаётся из браузера.
            var elapsedSeconds = Math.Max(
                0,
                (int)(DateTime.UtcNow - session.StartedOn).TotalSeconds);

            var timeExpired = definition.TimeLimitSeconds.HasValue
                && elapsedSeconds > definition.TimeLimitSeconds.Value;

            GameSolutionResult result;

            if (timeExpired)
            {
                result = new GameSolutionResult(
                    false,
                    0,
                    "Время истекло. Попробуйте пройти уровень ещё раз.");
            }
            else
            {
                result = this.gameService.CheckSolution(
                    definition,
                    input.SelectedNumberIds);
            }

            // Любой неуспешный ответ увеличивает число ошибок.
            if (!result.IsCorrect)
            {
                session.FailedAttempts++;
            }

            // После успеха или истечения времени эта сессия закрывается.
            if (result.IsCorrect || timeExpired)
            {
                session.IsCompleted = true;
            }

            var score = this.scoreCalculator.Calculate(
                isSuccessful: result.IsCorrect && !timeExpired,
                totalChips: definition.Numbers.Count,
                usedChips: input.SelectedNumberIds?.Count ?? 0,
                elapsedSeconds: elapsedSeconds,
                failedAttempts: session.FailedAttempts,
                timeLimitSeconds: definition.TimeLimitSeconds);

            // Сохраняем каждую отправку для будущей статистики.
            var attempt = new GameAttempt
            {
                UserId = session.UserId,
                LevelNumber = session.LevelNumber,
                TargetNumber = definition.TargetNumber,
                ResultNumber = result.CalculatedResult,
                IsSuccessful = result.IsCorrect && !timeExpired,
                Score = score,
                MovesCount = input.SelectedNumberIds?.Count ?? 0,
                ElapsedSeconds = elapsedSeconds,
                FailedAttempts = session.FailedAttempts,
                PlayedOn = DateTime.UtcNow,
            };

            await this.gameAttemptRepository.AddAsync(attempt);

            // Репозитории используют общий scoped DbContext,
            // поэтому вместе сохраняются и сессия, и запись попытки.
            await this.gameAttemptRepository.SaveChangesAsync();

            // var model = this.CreateViewModel(
            //     definition,
            //     session,
            //     elapsedSeconds);

            // model.SelectedNumberIds = input.SelectedNumberIds ?? new();
            // model.CalculatedResult = result.CalculatedResult;
            // model.FeedbackMessage = result.Message;
            // model.IsLevelCompleted = result.IsCorrect && !timeExpired;


            //return this.View("Index", model);

            // Создаём модель для отображения результата текущей попытки.
            var viewModel = this.CreateViewModel(
                definition,
                session,
                elapsedSeconds);

            viewModel.SelectedNumberIds = input.SelectedNumberIds ?? new();
            viewModel.CalculatedResult = result.CalculatedResult;
            viewModel.FeedbackMessage = result.Message;

            // Успешное прохождение уровня.
            if (result.IsCorrect)
            {
                viewModel.IsLevelCompleted = true;
                viewModel.IsAttemptFinished = false;
            }
            else if (timeExpired)
            {
                // Время истекло — попытка окончательно завершена.
                viewModel.IsLevelCompleted = false;
                viewModel.IsAttemptFinished = true;
            }

            return this.View("Index", viewModel);
        }

        /// <summary>
        /// Преобразует сохранённый уровень и состояние сессии в модель страницы.
        /// </summary>
        private GameViewModel CreateViewModel(
            GameLevelDefinition definition,
            GameSession session,
            int elapsedSeconds)
        {
            return new GameViewModel
            {
                SessionId = session.Id,
                LevelNumber = definition.LevelNumber,
                TargetNumber = definition.TargetNumber,
                Title = definition.Title,
                Rules = definition.Rules,
                TimeLimitSeconds = definition.TimeLimitSeconds,
                ElapsedSeconds = elapsedSeconds,
                FailedAttempts = session.FailedAttempts,
                Numbers = definition.Numbers
                    .Select(number => new GameNumberViewModel
                    {
                        Id = number.Id,
                        Value = number.Value,
                    })
                    .ToList(),
                Operation = definition.Operation,
                RequiredNumberId = definition.RequiredNumberId,
            };
        }
    }
}
