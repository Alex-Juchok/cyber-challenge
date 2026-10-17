namespace CyberChallenge.Services.Data.Game
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CyberChallenge.Services.Data.Game.Models;

    /// <summary>
    /// Реализует правила уровней и проверку решений.
    /// </summary>
    public sealed class GameService : IGameService
    {
        private static readonly Random Rand = new Random();

        /// <summary>
        /// Реализует генерацию случайного уровня.
        /// </summary>
        private static GameLevelDefinition GenerateRandomLevel(int levelNumber, string title, string description)
        {
            int totalChipsCount = Rand.Next(5, 15);

            // Определяет размер выигрышной комбинации (например, от 2 до 4 фишек)
            int winningChipsCount = Rand.Next(2, 5);
            if (winningChipsCount > totalChipsCount) winningChipsCount = totalChipsCount;

            var winningChips = new List<int>();
            var allChips = new List<int>();

            // Диапазон случайных чисел на фишках (можно менять в зависимости от уровня)
            int minVal = levelNumber >= 4 ? -10 : 1;
            int maxVal = 15;

            // Генерирует уникальные выигрышные фишки
            while (winningChips.Count < winningChipsCount)
            {
                int num = Rand.Next(minVal, maxVal + 1);
                if (num != 0 && !winningChips.Contains(num))
                {
                    winningChips.Add(num);
                }
            }

            // Целевое число уровня — это сумма выигрышной комбинации
            int targetNumber = winningChips.Sum();

            allChips.AddRange(winningChips);

            // Заполняет оставшиеся фишки «шумом» (ложными числами)
            while (allChips.Count < totalChipsCount)
            {
                int fakeNum = Rand.Next(minVal, maxVal + 1);
                // Избегает дубликатов, чтобы фишки были уникальными
                if (!allChips.Contains(fakeNum) && fakeNum != 0)
                {
                    allChips.Add(fakeNum);
                }
            }

            // Перемешивает фишки, чтобы выигрышная комбинация не шла в самом начале
            allChips = allChips.OrderBy(_ => Rand.Next()).ToList();

            // Возвращаем готовый уровень (передаем массив фишек через параметры)
            return CreateLevel(levelNumber, targetNumber, title, description, allChips.ToArray());
        }

        // Описания уровней хранятся централизованно.
        private static readonly IReadOnlyDictionary<int, GameLevelDefinition> Levels =
    new Dictionary<int, GameLevelDefinition>
    {
        [1] = GenerateRandomLevel(1, "Первый шаг", "Сложите положительные числа. Каждую фишку можно использовать один раз."),
        [2] = GenerateRandomLevel(2, "Точный расчёт", "Получите целевое число с помощью доступных фишек."),
        [3] = GenerateRandomLevel(3, "Больше возможностей", "Выбирайте подходящие числа. Повторно использовать фишку нельзя."),
        [4] = GenerateRandomLevel(4, "Отрицательный баланс", "Отрицательные числа уменьшают сумму."),
        [5] = GenerateRandomLevel(5, "Баланс со знаком", "Учитывайте знак каждого числа."),
        [6] = GenerateRandomLevel(6, "Ниже нуля", "Используйте положительные и отрицательные числа."),
        [7] = GenerateRandomLevel(7, "Сложный баланс", "Найдите комбинацию, дающую нужную сумму."),
        [8] = GenerateRandomLevel(8, "Финальная проверка", "Каждое число можно выбрать только один раз.")
    };

        /// <inheritdoc />
        public GameLevelDefinition? GetLevel(int levelNumber)
        {
            return Levels.TryGetValue(levelNumber, out var level)
                ? level
                : null;
        }

        /// <inheritdoc />
        public GameSolutionResult CheckSolution(
            int levelNumber,
            IReadOnlyCollection<string> selectedNumberIds)
        {
            // Нельзя проверять решение для несуществующего уровня.
            var level = this.GetLevel(levelNumber);

            if (level == null)
            {
                return new GameSolutionResult(
                    false,
                    0,
                    "Уровень не найден.");
            }

            // Пустой выбор не является решением.
            if (selectedNumberIds == null || selectedNumberIds.Count == 0)
            {
                return new GameSolutionResult(
                    false,
                    0,
                    "Переместите хотя бы одну фишку в контейнер.");
            }

            // Идентификаторы должны принадлежать текущему уровню.
            // Distinct также позволяет обнаружить повторную отправку одной фишки.
            var selectedIds = selectedNumberIds.ToArray();

            if (selectedIds.Distinct().Count() != selectedIds.Length)
            {
                return new GameSolutionResult(
                    false,
                    0,
                    "Нельзя использовать одну фишку несколько раз.");
            }

            var availableNumbers = level.Numbers.ToDictionary(number => number.Id);

            if (selectedIds.Any(id => !availableNumbers.ContainsKey(id)))
            {
                return new GameSolutionResult(
                    false,
                    0,
                    "Выбраны недопустимые фишки.");
            }

            // Сервер самостоятельно рассчитывает сумму по исходным данным уровня.
            var calculatedResult = selectedIds
                .Select(id => availableNumbers[id].Value)
                .Sum();

            var isCorrect = calculatedResult == level.TargetNumber;

            return new GameSolutionResult(
                isCorrect,
                calculatedResult,
                isCorrect
                    ? "Верно! Уровень пройден."
                    : $"Получилось {calculatedResult}. Попробуйте ещё раз.");
        }

        /// <summary>
        /// Создаёт уровень с уникальными идентификаторами фишек.
        /// Идентификаторы стабильны между запросами для одного уровня.
        /// </summary>
        private static GameLevelDefinition CreateLevel(
            int levelNumber,
            int targetNumber,
            string title,
            string rules,
            params int[] values)
        {
            var numbers = values
                .Select((value, index) => new GameNumberDefinition(
                    $"level-{levelNumber}-number-{index + 1}",
                    value))
                .ToArray();

            return new GameLevelDefinition(
                levelNumber,
                targetNumber,
                numbers,
                title,
                rules);
        }
    }
}