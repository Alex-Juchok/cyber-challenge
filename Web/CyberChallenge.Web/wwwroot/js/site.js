
(() => {
    const bank = document.getElementById("number-bank");
    const container = document.getElementById("number-container");
    const placeholder = document.getElementById("container-placeholder");
    const resultElement = document.getElementById("current-sum");
    const hiddenInputs = document.getElementById("selected-number-inputs");
    const clearButton = document.getElementById("clear-selection");
    const form = document.getElementById("game-form");
    const timerElement = document.getElementById("game-timer");
    const config = document.getElementById("game-config");

    if (!bank || !container || !placeholder || !resultElement ||
        !hiddenInputs || !clearButton || !form || !config) {
        return;
    }

    // Операция берётся из серверной модели уровня.
    const operation = config.dataset.operation;
    const timeLimit = Number(config.dataset.timeLimit);
    let elapsedSeconds = Number(config.dataset.elapsedSeconds);
    let finished = config.dataset.finished === "true";

    // Map сохраняет порядок добавления фишек.
    const selected = new Map();

    const previouslySelectedIds = Array.from(
        hiddenInputs.querySelectorAll("input[name='SelectedNumberIds']")
    ).map(input => input.value);

    function findChip(id) {
        return Array.from(
            document.querySelectorAll(".number-chip")
        ).find(chip => chip.dataset.numberId === id);
    }

    // Синхронизируем Map с фактическим порядком фишек в контейнере.
    function syncSelectedOrder() {
        selected.clear();

        container.querySelectorAll(".number-chip").forEach(chip => {
            selected.set(chip.dataset.numberId, chip);
        });
    }

    // Предварительно рассчитываем результат в том же порядке,
    // в котором фишки расположены в контейнере.
    function calculatePreview(values) {
        if (values.length === 0) {
            return 0;
        }

        switch (operation) {
            case "Addition":
                return values.reduce((total, value) => total + value, 0);

            case "Subtraction":
                return values.slice(1).reduce(
                    (result, value) => result - value,
                    values[0]
                );

            case "Multiplication":
                return values.reduce((result, value) => result * value, 1);

            case "Combined":
                // Для комбинированного режима нужно хранить операцию
                // между каждой парой чисел. Пока не имитируем неверную формулу.
                return null;

            default:
                return null;
        }
    }

    function refreshInterface() {
        syncSelectedOrder();

        const values = Array.from(selected.values())
            .map(chip => Number(chip.dataset.value));

        const preview = calculatePreview(values);

        resultElement.textContent =
            preview === null ? "—" : String(preview);

        placeholder.hidden = selected.size > 0;

        // Скрытые поля создаются в том же порядке, что и фишки.
        hiddenInputs.replaceChildren();

        selected.forEach(chip => {
            const input = document.createElement("input");
            input.type = "hidden";
            input.name = "SelectedNumberIds";
            input.value = chip.dataset.numberId;
            hiddenInputs.appendChild(input);
        });

        document.querySelectorAll(".number-chip").forEach(chip => {
            const isSelected = selected.has(chip.dataset.numberId);

            chip.classList.toggle("is-selected", isSelected);
            chip.setAttribute("aria-pressed", String(isSelected));
        });
    }

    function moveChip(chip, destination) {
        if (finished) {
            return;
        }

        destination.appendChild(chip);
        refreshInterface();
    }

    document.querySelectorAll(".number-chip").forEach(chip => {
        chip.addEventListener("click", () => {
            if (finished) {
                return;
            }

            const destination = selected.has(chip.dataset.numberId)
                ? bank
                : container;

            moveChip(chip, destination);
        });

        chip.addEventListener("dragstart", event => {
            if (finished) {
                event.preventDefault();
                return;
            }

            event.dataTransfer.setData(
                "text/plain",
                chip.dataset.numberId
            );

            event.dataTransfer.effectAllowed = "move";
        });
    });

    [bank, container].forEach(area => {
        area.addEventListener("dragover", event => {
            if (finished) {
                return;
            }

            event.preventDefault();

            if (area === container) {
                container.classList.add("is-drop-target");
            }
        });

        area.addEventListener("dragleave", event => {
            if (!area.contains(event.relatedTarget)) {
                container.classList.remove("is-drop-target");
            }
        });

        area.addEventListener("drop", event => {
            event.preventDefault();
            container.classList.remove("is-drop-target");

            if (finished) {
                return;
            }

            const id = event.dataTransfer.getData("text/plain");
            const chip = findChip(id);

            if (chip) {
                moveChip(chip, area);
            }
        });
    });

    clearButton.addEventListener("click", () => {
        if (finished) {
            return;
        }

        Array.from(container.querySelectorAll(".number-chip"))
            .forEach(chip => bank.appendChild(chip));

        refreshInterface();
    });

    // Восстанавливаем выбор после ответа сервера.
    previouslySelectedIds.forEach(id => {
        const chip = findChip(id);

        if (chip) {
            container.appendChild(chip);
        }
    });

    function showGameOver() {
        if (finished) {
            return;
        }

        finished = true;

        // Блокируем взаимодействие с уже завершённой попыткой.
        form.querySelectorAll("button").forEach(button => {
            button.disabled = true;
        });

        document.querySelectorAll(".number-chip").forEach(chip => {
            chip.draggable = false;
        });

        const overlay = document.createElement("div");
        overlay.className = "game-over-screen";
        overlay.setAttribute("role", "alert");

        const card = document.createElement("div");
        card.className = "game-over-card";

        const heading = document.createElement("h1");
        heading.textContent = "Попытка завершена";

        const message = document.createElement("p");
        message.textContent =
            "Время истекло. Начните уровень заново, чтобы попробовать ещё раз.";

        const restart = document.createElement("a");
        restart.className = "next-level-button";
        restart.href = window.location.pathname +
            "?level=" + encodeURIComponent(config.dataset.level);
        restart.textContent = "Начать заново";

        card.append(heading, message, restart);
        overlay.appendChild(card);
        document.body.appendChild(overlay);
    }

    // Клиентский таймер отвечает за отображение и блокировку интерфейса.
    // Источник достоверного времени всё равно находится на сервере.
    function renderTimer() {
        if (!timerElement || finished) {
            return;
        }

        const displayedSeconds = timeLimit >= 0
            ? Math.max(0, timeLimit - elapsedSeconds)
            : elapsedSeconds;

        timerElement.textContent =
            String(Math.floor(displayedSeconds / 60)).padStart(2, "0") +
            ":" +
            String(displayedSeconds % 60).padStart(2, "0");

        if (timeLimit >= 0 && displayedSeconds <= 10) {
            timerElement.classList.add("timer-warning");
        }

        if (timeLimit >= 0 && elapsedSeconds >= timeLimit) {
            showGameOver();
            return;
        }

        elapsedSeconds++;
    }

    // Защищаемся от отправки формы после завершения попытки.
    form.addEventListener("submit", event => {
        if (finished) {
            event.preventDefault();
        }
    });

    refreshInterface();
    renderTimer();
    window.setInterval(renderTimer, 1000);
})();
