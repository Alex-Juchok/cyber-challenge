// Содержимое файла wwwroot/js/timer.js
(() => {
    const timerElement = document.getElementById('game-config');

    if (!timerElement) {
        return;
    }

    // Читаем начальные значения из HTML-атрибутов, которые сгенерировал C# сервер
    let elapsedSeconds = Number(timerElement.dataset.elapsed-seconds) || 0;
    const timeLimit = Number(timerElement.dataset.time-limit) ?? -1;

    function renderTimer() {
        const displayedSeconds = timeLimit >= 0
            ? Math.max(0, timeLimit - elapsedSeconds)
            : elapsedSeconds;

        const minutes = Math.floor(displayedSeconds / 60);
        const seconds = displayedSeconds % 60;

        timerElement.textContent =
            String(minutes).padStart(2, '0') + ':' +
            String(seconds).padStart(2, '0');

        // Предупреждаем игрока, когда время заканчивается.
        if (timeLimit >= 0 && displayedSeconds <= 10) {
            timerElement.classList.add('timer-warning');
        }

        elapsedSeconds++;
    }

    renderTimer();
    window.setInterval(renderTimer, 1000);
})();
