// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
    (() => {
        const bank = document.getElementById("number-bank");
        const container = document.getElementById("number-container");
        const placeholder = document.getElementById("container-placeholder");
        const sumElement = document.getElementById("current-sum");
        const hiddenInputs = document.getElementById("selected-number-inputs");
        const clearButton = document.getElementById("clear-selection");

        if (!bank || !container || !placeholder || !sumElement || !hiddenInputs || !clearButton) {
            return;
        }

        // Храним выбранные фишки по ID, чтобы не допускать дубликатов.
        const selected = new Map();

        // Восстанавливаем выбор после ответа сервера.
        const previouslySelectedIds = Array.from(
            hiddenInputs.querySelectorAll("input[name='SelectedNumberIds']")
        ).map(input => input.value);

        function findChip(id) {
            return document.querySelector(
                `.number-chip[data-number-id="${CSS.escape(id)}"]`
            );
        }

        function refreshInterface() {
            let total = 0;

            selected.forEach(chip => {
                total += Number(chip.dataset.value);
            });

            sumElement.textContent = String(total);
            placeholder.hidden = selected.size > 0;

            // Перестраиваем скрытые поля, отправляемые серверу.
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
            const id = chip.dataset.numberId;

            if (destination === container) {
                selected.set(id, chip);
                container.appendChild(chip);
            } else {
                selected.delete(id);
                bank.appendChild(chip);
            }

            refreshInterface();
        }

        document.querySelectorAll(".number-chip").forEach(chip => {
            chip.addEventListener("click", () => {
                const destination = selected.has(chip.dataset.numberId)
                    ? bank
                    : container;

                moveChip(chip, destination);
            });

            chip.addEventListener("dragstart", event => {
                event.dataTransfer.setData("text/plain", chip.dataset.numberId);
                event.dataTransfer.effectAllowed = "move";
            });
        });

        [bank, container].forEach(area => {
            area.addEventListener("dragover", event => {
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

                const id = event.dataTransfer.getData("text/plain");
                const chip = findChip(id);

                if (chip) {
                    moveChip(chip, area);
                }
            });
        });

        clearButton.addEventListener("click", () => {
            Array.from(selected.values()).forEach(chip => bank.appendChild(chip));
            selected.clear();
            refreshInterface();
        });

        previouslySelectedIds.forEach(id => {
            const chip = findChip(id);

            if (chip) {
                selected.set(id, chip);
                container.appendChild(chip);
            }
        });

        refreshInterface();
    })();