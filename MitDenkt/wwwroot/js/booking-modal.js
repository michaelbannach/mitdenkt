
// Notwendig um das Buchungsmodal zu öffnen, und enthält info, sercvices und calendar
// Holt DOM-elemente
export function setupBookingModal(info, services, calendar) {
    const modal = document.getElementById("serviceModal");
    const modalContent = document.getElementById("modalContent");
    const priceDisplay = document.getElementById("priceTotal");
    modal.style.display = "flex";


    // Kategorien werden in gewünschter Reihenfolge im Modal dargestellt
    const categoryOrder = ["Hauptdienstleistung", "Farbe", "Pflege", "Extra"];

    const categories = categoryOrder
        .map(cat => [cat, services.filter(s => s.category === cat)])
        .filter(([, items]) => items.length > 0);

    
    // Baut dynamisch HTML-Formular für Dienstleistungsauswahl
    // Für jede Kategorie wird ein Header und eine Liste mit Checkboxen erzeugt
    modalContent.innerHTML = `
        <form id="serviceForm">
        ${categories.map(([category, items]) => `
            <div class="category">
                <div class="category-header">${category}</div>
                <div class="category-body">
                    <ul>${items.map(s => `
                        <li>
                            <label>
                                <input type="checkbox" name="service" value="${s.id}" data-price="${s.price}" />
                                ${s.name} (${s.price.toFixed(2)} €)
                            </label>
                        </li>
                    `).join("")}</ul>
                </div>
            </div>
        `).join("")}
        <div class="category">
            <div class="category-header">Bemerkung (optional)</div>
            <div class="category-body">
                <textarea name="note" rows="3" style="width: 100%;"></textarea>
            </div>
        </div>
        </form>`;

    // Wenn Schließen-Button gedrückt wird -> Modal ausgeblendet und der Preis wird zurückgesetzt
    document.getElementById("modalCloseBtn").onclick = () => {
        modal.style.display = "none";
        priceDisplay.textContent = "Summe: 0,00 €";
    };

    // Buchung absenden
    // Sendet Post-Anfrage an /api/booking -> enthält employeeId, startTime und serviceIds
    
    document.getElementById("bookBtn").onclick = () => {
        const form = document.getElementById("serviceForm");
        const selected = [...form.querySelectorAll('input[name="service"]:checked')].map(cb => parseInt(cb.value));
        if (selected.length === 0) return alert("Bitte Dienstleistung auswählen.");
        fetch("/api/booking", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify({
                employeeId: info.resource.id,
                startTime: info.startStr,
                serviceIds: selected
            })
            
            //wenn erfolgreich -> Termin erscheint im Kalendar
        }).then(res => {
            if (res.ok) {
                calendar.refetchEvents();
                modal.style.display = "none";
                priceDisplay.textContent = "Summe: 0,00 €";
            } else res.text().then(alert);
        });
    };

    // Hört auf Änderungen innerhalb des Modalformulars, addiert data-price Werte aller aktivierten Checkboxen
    modalContent.addEventListener("change", () => {
        const total = [...modalContent.querySelectorAll('input[name="service"]:checked')]
            .reduce((sum, cb) => sum + parseFloat(cb.dataset.price || 0), 0);
        priceDisplay.textContent = `Summe: ${total.toFixed(2)} €`;
    });
}
