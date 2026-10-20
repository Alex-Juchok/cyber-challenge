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
            // 1. Инициализация базовых параметров в зависимости от уровня
            int totalChipsCount = Rand.Next(6, 12);
            int winningChipsCount = Rand.Next(2, 5);

            int minVal = 1;
            int maxVal = 15;
            GameOperation operation = GameOperation.Addition;
            int? timeLimitSeconds = null;
            string? requiredNumberId = null;

            // 2. Настройка правил для диапазонов уровней
            if (levelNumber >= 3 && levelNumber <= 4)
            {
                // 3–4: Отрицательные числа
                minVal = -10;
            }
            else if (levelNumber >= 5 && levelNumber <= 6)
            {
                // 5–6: Вычитание
                minVal = -5; // добавим немного отрицательных для интереса
                operation = GameOperation.Subtraction;
            }
            else if (levelNumber >= 7 && levelNumber <= 8)
            {
                // 7–8: Умножение (уменьшаем фишки, чтобы числа не улетали в бесконечность)
                winningChipsCount = Rand.Next(2, 4);
                operation = GameOperation.Multiplication;
            }
            else if (levelNumber >= 9 && levelNumber <= 10)
            {
                // 9–10: Комбинированные операции (например, кастомная сложная операция или Multiply)
                operation = Rand.Next(0, 2) == 0 ? GameOperation.Multiplication : GameOperation.Addition;
            }
            else if (levelNumber >= 13 && levelNumber <= 14)
            {
                // 13–14: Ограничение времени
                timeLimitSeconds = Rand.Next(30, 61); // от 30 до 60 секунд
            }
            else if (levelNumber >= 15)
            {
                // 15+: Смешанные испытания (случайные модификаторы)
                minVal = -10;
                operation = (GameOperation)Rand.Next(0, Enum.GetValues(typeof(GameOperation)).Length);
                if (Rand.Next(0, 2) == 1) timeLimitSeconds = Rand.Next(20, 45);
            }

            if (winningChipsCount > totalChipsCount) winningChipsCount = totalChipsCount;

            // 3. Генерация уникальной выигрышной комбинации чисел
            var winningValues = new List<int>();
            while (winningValues.Count < winningChipsCount)
            {
                int num = Rand.Next(minVal, maxVal + 1);
                // Избегаем нуля (чтобы не ломать умножение/деление) и дубликатов
                if (num != 0 && !winningValues.Contains(num))
                {
                    winningValues.Add(num);
                }
            }

            // 4. Расчёт целевого числа (TargetNumber) на основе выбранной операции
            int targetNumber = winningValues[0];
            for (int i = 1; i < winningValues.Count; i++)
            {
                targetNumber = operation switch
                {
                    GameOperation.Addition => targetNumber + winningValues[i],
                    GameOperation.Subtraction => targetNumber - winningValues[i],
                    GameOperation.Multiplication => targetNumber * winningValues[i],
                    _ => targetNumber + winningValues[i]
                };
            }

            // 5. Заполнение оставшихся фишек "шумом"
            var allValues = new List<int>(winningValues);
            while (allValues.Count < totalChipsCount)
            {
                int fakeNum = Rand.Next(minVal, maxVal + 1);
                if (!allValues.Contains(fakeNum) && fakeNum != 0)
                {
                    allValues.Add(fakeNum);
                }
            }

            // Перемешиваем значения
            allValues = allValues.OrderBy(_ => Rand.Next()).ToList();

            // 6. Маппинг чисел в объекты GameNumberDefinition с генерацией ID
            var numbers = allValues.Select((val, index) => new GameNumberDefinition(
                id: $"chip_{levelNumber}_{index}",
                value: val
            )).ToList();

            // 11–12: Обязательные фишки (выбираем одну из выигрышных фишек как обязательную)
            if (levelNumber >= 11 && levelNumber <= 12 || (levelNumber >= 15 && Rand.Next(0, 2) == 1))
            {
                int targetWinValue = winningValues[Rand.Next(0, winningValues.Count)];
                var requiredChip = numbers.First(n => n.Value == targetWinValue);
                requiredNumberId = requiredChip.Id;
            }

            // 7. Создание и возврат итогового объекта уровня
            return new GameLevelDefinition(
                levelNumber,
                targetNumber,
                numbers,
                title,
                description, // Передаем описание в параметр rules
                operation,
                timeLimitSeconds,
                requiredNumberId
            );
        }



        /// <summary>
        /// Рассчитывает результат выбранных фишек с учётом операции уровня.
        /// </summary>
        private static int CalculateResult(
            GameOperation operation,
            IReadOnlyCollection<int> values)
        {
            // Сохраняем порядок: для вычитания он имеет значение.
            var numbers = values.ToArray();

            if (numbers.Length == 0)
            {
                return 0;
            }

            return operation switch
            {
                GameOperation.Addition => numbers.Sum(),

                GameOperation.Subtraction =>
                    numbers.Skip(1).Aggregate(
                        numbers[0],
                        (result, number) => checked(result - number)),

                GameOperation.Multiplication =>
                    numbers.Aggregate(
                        1,
                        (result, number) => checked(result * number)),

                // Комбинированные операции реализуем отдельно,
                // поскольку для них нужен порядок операций и приоритеты.
                GameOperation.Combined =>
                    throw new NotSupportedException(
                        "Комбинированная операция пока не реализована."),

                _ => throw new ArgumentOutOfRangeException(nameof(operation)),
            };
        }


        // Описания уровней хранятся централизованно.
        private static readonly IReadOnlyDictionary<int, GameLevelDefinition> Levels =
    new Dictionary<int, GameLevelDefinition>
    {
        // 1–2: Положительные числа (Сложение положительных фишек)
        [1] = GenerateRandomLevel(1, "Первый шаг", "Сложите положительные числа. Каждую фишку можно использовать один раз."),
        [2] = GenerateRandomLevel(2, "Точный расчёт", "Получите целевое число с помощью доступных положительных фишек."),

        // 3–4: Отрицательные числа (Сложение положительных и отрицательных)
        [3] = GenerateRandomLevel(3, "Отрицательный баланс", "В игре появились отрицательные числа. Они уменьшают общую сумму!"),
        [4] = GenerateRandomLevel(4, "Баланс со знаком", "Учитывайте знак каждого числа при сложении, чтобы выйти на точную цель."),

        // 5–6: Вычитание (Порядок выбора фишек влияет на результат)
        [5] = GenerateRandomLevel(5, "Обратный отсчёт", "Режим вычитания! Каждая следующая выбранная фишка вычитается из предыдущей."),
        [6] = GenerateRandomLevel(6, "Важен порядок", "Будьте внимательны: порядок выбора фишек при вычитании полностью меняет итог."),

        // 7–8: Умножение (Произведение выбранных чисел)
        [7] = GenerateRandomLevel(7, "Фактор роста", "Режим умножения! Выбранные числа перемножаются между собой."),
        [8] = GenerateRandomLevel(8, "Масштабирование", "Быстрый рост чисел. Подберите правильные множители для достижения цели."),

        // 9–10: Комбинированные операции (Например, сложение и умножение)
        [9] = GenerateRandomLevel(9, "Смешанный режим", "Внимание на знак операции вверху экрана! Здесь могут быть как сумма, так и произведение."),
        [10] = GenerateRandomLevel(10, "Двойной стандарт", "Операция выбирается случайно. Просчитайте стратегию под текущий режим."),

        // 11–12: Обязательные фишки (Нужно использовать определённую фишку)
        [11] = GenerateRandomLevel(11, "Особое условие", "Среди фишек есть обязательная (подсвечена на экране). Решение должно включать её!"),
        [12] = GenerateRandomLevel(12, "Главный элемент", "Вы не сможете завершить уровень, если не задействуете обязательную фишку."),

        // 13–14: Ограничение времени (Решение за заданное количество секунд)
        [13] = GenerateRandomLevel(13, "Быстрый ум", "Время пошло! На этом уровне включён таймер. Поторопитесь с решением."),
        [14] = GenerateRandomLevel(14, "Цейтнот", "Секунды тают! Успейте найти верную комбинацию до того, как время истечёт."),

        // 15+: Смешанные испытания (Несколько правил одновременно)
        [15] = GenerateRandomLevel(15, "Гранд-мастер", "Испытание для экспертов: случайная операция, ограничение по времени и обязательная фишка!"),
        [16] = GenerateRandomLevel(16, "Абсолютный хаос", "Все правила перепутаны. Сможете ли вы справиться со случайным набором ограничений?")
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
            GameLevelDefinition levelNumber,
            IReadOnlyCollection<string> selectedNumberIds)
        {
            // Нельзя проверять решение для несуществующего уровня.

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

            var availableNumbers = levelNumber.Numbers.ToDictionary(number => number.Id);

            if (selectedIds.Any(id => !availableNumbers.ContainsKey(id)))
            {
                return new GameSolutionResult(
                    false,
                    0,
                    "Выбраны недопустимые фишки.");
            }


            // Сервер самостоятельно рассчитывает сумму по исходным данным уровня.
            var selectedValues = selectedIds
                    .Select(id => availableNumbers[id].Value)
                    .ToArray();

            int calculatedResult;

            try
            {
                calculatedResult = CalculateResult(levelNumber.Operation, selectedValues);
            }
            catch (OverflowException)
            {
                return new GameSolutionResult(
                    false,
                    0,
                    "Результат слишком велик. Попробуйте другую комбинацию.");
            }

            // Сверяем то, что посчитал сервер, с целью (TargetNumber) этого уровня
            bool isSuccess = calculatedResult == levelNumber.TargetNumber;
            string message = isSuccess
                ? "Поздравляем! Верное решение."
                : $"Неверно. Вы получили {calculatedResult}, а цель уровня: {levelNumber.TargetNumber}.";

            return new GameSolutionResult(
                isSuccess,
                calculatedResult,
                message);
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
                rules,
                GameOperation.Addition,
                10, null);
        }
    }


}