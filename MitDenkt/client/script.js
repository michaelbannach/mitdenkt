
document.addEventListener('DOMContentLoaded', async function () {
    const calendarEl = document.getElementById('calendar');
    const dateLabel = document.getElementById('date-label');

    const resourceRes = await fetch('http://localhost:5057/api/employee', {credentials: 'include'});
    const resources = await resourceRes.json();

    const serviceRes = await fetch('http://localhost:5057/api/service', {credentials: 'include'});
    const services = await serviceRes.json();

    const employees = {
        "Tina Müller": {
            color: '#e91e63',
            image: '/img/portrait_1.png'
        },
        "Peter Meier": {
            color: '#4caf50',
            image: '/img/portrait_2.png'
        },
        "Sonja Klein": {
            color: '#2196f3',
            image: '/img/portrait_3.png'
        },
        "Michael Berger": {
            color: '#ff5722',
            image: '/img/portrait_4.png'
        }
    };

    const calendar = new FullCalendar.Calendar(calendarEl, {
        schedulerLicenseKey: 'CC-Attribution-NonCommercial-NoDerivatives',
        initialView: 'resourceTimeGridDay',
        locale: 'de',
        allDaySlot: false,
        headerToolbar: false,
        slotDuration: '00:15:00',
        slotLabelInterval: '01:00',
        slotMinTime: '08:00:00',
        slotMaxTime: '18:00:00',
        contentHeight: 'auto',
        slotLabelFormat: {hour: '2-digit', minute: '2-digit', hour12: false},
        selectable: true,
        editable: false,
        titleFormat: {weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'},

        resourceLabelContent: function (arg) {
            const name = arg.resource.title;
            const employee = employees[name] || {};
            const color = employee.color || '#ccc';
            const imgSrc = employee.image || '/img/default.png';

            return {
                html: `
                    <div class="resource-label">
                        <div class="resource-name">
                            <span style="color:${color}; font-weight: bold;">●</span>
                            <span style="font-weight: 600;">${name}</span>
                        </div>
                        <img src="${imgSrc}" alt="${name}" class="resource-portrait" data-preview="${imgSrc}" />
                    </div>
                `
            };
        },

        resources: resources.map(e => ({
            id: e.id,
            title: `${e.firstName} ${e.lastName}`
        })),

        events: function (fetchInfo, successCallback, failureCallback) {
            const date = fetchInfo.startStr.split('T')[0];
            fetch('http://localhost:5057/api/booking/date/' + date, {credentials: 'include'})
                .then(res => res.json())
                .then(data => {
                    const events = data.map(b => ({
                        id: b.id,
                        title: b.employeeName,
                        start: b.startTime,
                        end: b.endTime,
                        resourceId: b.employeeId,
                        color: employeeColors[b.employeeName] || '#888'
                    }));
                    successCallback(events);
                })
                .catch(failureCallback);
        },

        select: function (info) {
            const modal = document.getElementById("serviceModal");
            const modalContent = document.getElementById("modalContent");
            const priceDisplay = document.getElementById("priceTotal");
            modal.style.display = "flex";

            const categories = Object.entries(
                services.reduce((acc, s) => {
                    if (!acc[s.category]) acc[s.category] = [];
                    acc[s.category].push(s);
                    return acc;
                }, {})
            );

            modalContent.innerHTML = `
                <form id="serviceForm">
                  ${categories.map(([category, items]) => `
                    <div class="category">
                      <div class="category-header">${category}</div>
                      <div class="category-body" style="display: block;">
                        <ul style="list-style: none; padding-left: 0; margin: 0;">
                          ${items.map((s) => `
                            <li style="margin-bottom: 0.5em;">
                              <label style="display: flex; align-items: center; gap: 0.5em;">
                                <input type="checkbox" name="service" value="${s.id}" data-price="${s.price}" />
                                ${s.name} (${s.price.toFixed(2)} €)
                              </label>
                            </li>
                          `).join("")}
                        </ul>
                      </div>
                    </div>
                  `).join("")}
                  <div class="category">
                    <div class="category-header">Bemerkungen (optional):</div>
                    <div class="category-body" style="display: block;">
                      <textarea name="note" rows="3" style="width: 100%;"></textarea>
                    </div>
                  </div>
                </form>`;

            document.getElementById("modalCloseBtn").onclick = () => {
                modal.style.display = "none";
                priceDisplay.textContent = "0,00 €";
            };
            document.getElementById("bookBtn").onclick = () => {
                const form = document.getElementById("serviceForm");
                const selected = Array.from(form.querySelectorAll('input[name="service"]:checked')).map(cb => parseInt(cb.value));
                if (selected.length === 0) {
                    alert("Bitte mindestens eine Dienstleistung auswählen.");
                    return;
                }
                fetch("http://localhost:5057/api/booking", {
                    method: "POST",
                    headers: {"Content-Type": "application/json"},
                    credentials: "include",
                    body: JSON.stringify({
                        employeeId: info.resource.id,
                        startTime: info.startStr,
                        endTime: info.endStr,
                        serviceIds: selected
                    })
                }).then((res) => {
                    if (res.ok) {
                        calendar.refetchEvents();
                        modal.style.display = "none";
                        priceDisplay.textContent = "Summe: 0,00 €"
                    } else {
                        res.text().then((msg) => alert("Fehler: " + msg));
                    }
                });
            };

            modalContent.addEventListener("change", () => {
                const checkboxes = modalContent.querySelectorAll('input[name="service"]:checked');
                let sum = 0;
                checkboxes.forEach(cb => sum += parseFloat(cb.dataset.price || 0));
                priceDisplay.textContent = `Summe: ${sum.toFixed(2)} €`;
            });
        },

        eventClick: function (info) {
            if (!confirm('Diesen Termin löschen?')) return;
            fetch('http://localhost:5057/api/booking/' + info.event.id, {
                method: 'DELETE',
                credentials: 'include'
            }).then(res => {
                if (res.ok) calendar.refetchEvents();
                else res.text().then(msg => alert('Fehler: ' + msg));
            });
        }
    });

    calendar.on('datesSet', function (info) {
        updateDateLabel(info.start);
    });

    calendar.render();
    let currentWeekStart = getStartOfWeek(new Date());
    renderWeekNav(calendar, currentWeekStart);

    const datepickerEl = document.createElement('div');
    datepickerEl.id = 'mini-calendar-popup';
    datepickerEl.style.position = 'absolute';
    datepickerEl.style.zIndex = '10000';
    datepickerEl.style.background = 'white';
    datepickerEl.style.border = '1px solid #ccc';
    datepickerEl.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
    datepickerEl.style.borderRadius = '6px';
    datepickerEl.style.padding = '0.5rem';
    datepickerEl.style.display = 'none';
    document.body.appendChild(datepickerEl);

    document.getElementById("calendar-trigger").addEventListener("click", (e) => {
        const rect = e.target.getBoundingClientRect();
        datepickerEl.style.left = `${rect.left + window.scrollX}px`;
        datepickerEl.style.top = `${rect.bottom + window.scrollY + 5}px`;
        datepickerEl.style.display = datepickerEl.style.display === 'none' ? 'block' : 'none';
        renderMiniCalendar(calendar.getDate());
    });


    function renderMiniCalendar(currentDate) {
        const today = new Date();
        const selectedMonth = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
        const firstDay = new Date(selectedMonth.getFullYear(), selectedMonth.getMonth(), 1).getDay();
        const daysInMonth = new Date(selectedMonth.getFullYear(), selectedMonth.getMonth() + 1, 0).getDate();

        let html = '<div style="text-align: center; margin-bottom: 5px;">' +
            `<button id="mini-prev">&lt;</button> ${selectedMonth.toLocaleDateString('de-DE', {month: 'long', year: 'numeric'})} <button id="mini-next">&gt;</button>` +
            '</div>';

        html += '<table style="width: 100%; text-align: center; font-size: 0.85rem;"><thead><tr>';
        const weekdays = ['Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa', 'So'];
        weekdays.forEach(d => html += `<th>${d}</th>`);
        html += '</tr></thead><tbody><tr>';

        let startOffset = (firstDay + 6) % 7; // Montag als erster Wochentag
        for (let i = 0; i < startOffset; i++) {
            html += '<td></td>';
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const thisDate = new Date(selectedMonth.getFullYear(), selectedMonth.getMonth(), day);
            const isToday = thisDate.toDateString() === today.toDateString();
            html += `<td style="padding: 3px; cursor: pointer;${isToday ? ' font-weight:bold; background:#eee;' : ''}" 
                    data-date="${thisDate.toISOString()}">${day}</td>`;

            if ((startOffset + day) % 7 === 0) {
                html += '</tr><tr>';
            }
        }

        html += '</tr></tbody></table>';
        datepickerEl.innerHTML = html;

        // Navigation
        document.getElementById('mini-prev').onclick = () => {
            const prevMonth = new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1);
            renderMiniCalendar(prevMonth);
        };
        document.getElementById('mini-next').onclick = () => {
            const nextMonth = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1);
            renderMiniCalendar(nextMonth);
        };

        // Klick auf ein Datum → Kalender aktualisieren
        Array.from(datepickerEl.querySelectorAll('[data-date]')).forEach(cell => {
            cell.addEventListener('click', () => {
                const picked = new Date(cell.dataset.date);
                calendar.gotoDate(picked);
                updateDateLabel(picked);
                renderWeekNav(calendar, getStartOfWeek(picked));
                datepickerEl.style.display = 'none';
            });
        });
    }





    function renderWeekNav(calendar, startDate) {
        currentWeekStart = startDate;
        const dayButtonsEl = document.getElementById('day-buttons');
        dayButtonsEl.innerHTML = '';
        for (let i = 0; i < 7; i++) {
            const day = new Date(startDate);
            day.setDate(startDate.getDate() + i);
            const btn = document.createElement('button');
            btn.className = 'day-button';
            btn.innerHTML = `${day.toLocaleDateString('de-DE', {weekday: 'short'})}
                <br>
                    ${String(day.getDate()).padStart(2, '0')}`;
            if (day.toDateString() === calendar.getDate().toDateString()) {
                btn.classList.add('active');
            }
            btn.onclick = () => {
                calendar.changeView('resourceTimeGridDay', day);
                renderWeekNav(calendar, currentWeekStart);
            };
            dayButtonsEl.appendChild(btn);
        }
    }

    document.getElementById('prev-week').onclick = () => {
        const prev = new Date(currentWeekStart);
        prev.setDate(prev.getDate() - 7);
        renderWeekNav(calendar, prev);
        calendar.changeView('resourceTimeGridDay', prev);
    };

    document.getElementById('next-week').onclick = () => {
        const next = new Date(currentWeekStart);
        next.setDate(next.getDate() + 7);
        renderWeekNav(calendar, next);
        calendar.changeView('resourceTimeGridDay', next);
    };

    function getStartOfWeek(date) {
        const d = new Date(date);
        const day = d.getDay();
        const diff = d.getDate() - ((day + 6) % 7);
        return new Date(d.setDate(diff));
    }

    function updateDateLabel(date) {
        if (!dateLabel) return;
        dateLabel.textContent = date.toLocaleDateString('de-DE', {
            weekday: 'long',
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    }
});
