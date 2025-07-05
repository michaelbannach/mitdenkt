import { getStartOfWeek } from './utils.js';

// Zeigt kleinen Monatskalendar und erlaubt Navigation und Datumsauswahl
export function renderMiniCalendar(currentDate, calendar, updateDateLabel, renderWeekNav) {
    const popup = document.getElementById('mini-calendar-popup');
    const today = new Date();
    const selectedMonth = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
    const firstDay = selectedMonth.getDay();
    const daysInMonth = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0).getDate();
    const startOffset = (firstDay + 6) % 7;

    let html = '<div style="text-align: center; margin-bottom: 5px;">';
    html += `<button id="mini-prev">&lt;</button> ${selectedMonth.toLocaleDateString('de-DE', { month: 'long', year: 'numeric' })} <button id="mini-next">&gt;</button>`;
    html += '</div><table style="width: 100%; text-align: center; font-size: 0.85rem;"><thead><tr>';

    ['Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa', 'So'].forEach(d => html += `<th>${d}</th>`);
    html += '</tr></thead><tbody><tr>';

    for (let i = 0; i < startOffset; i++) html += '<td></td>';
    for (let day = 1; day <= daysInMonth; day++) {
        const thisDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), day);
        const isToday = thisDate.toDateString() === today.toDateString();
        html += `<td style="cursor:pointer; padding:3px;${isToday ? ' background:#eee;' : ''}" data-date="${thisDate.toISOString()}">${day}</td>`;
        if ((startOffset + day) % 7 === 0) html += '</tr><tr>';
    }

    html += '</tr></tbody></table>';
    popup.innerHTML = html;

    // <-Button ist deaktiviert, wenn man sich im aktuellen Monat befindet
    const prevBtn = document.getElementById('mini-prev');
    const isCurrentMonth = currentDate.getFullYear() === today.getFullYear() &&
        currentDate.getMonth() === today.getMonth();

    if (isCurrentMonth) {
        prevBtn.disabled = true;
        prevBtn.style.opacity = '0.3';
        prevBtn.style.cursor = 'default';
    } else {
        prevBtn.disabled = false;
        prevBtn.style.opacity = '1';
        prevBtn.style.cursor = 'pointer';
        prevBtn.onclick = () => {
            const prevMonth = new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1);
            renderMiniCalendar(prevMonth, calendar, updateDateLabel, renderWeekNav);
        };
    }
    
    document.getElementById('mini-next').onclick = () => renderMiniCalendar(new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1), calendar, updateDateLabel, renderWeekNav);

    Array.from(popup.querySelectorAll('[data-date]')).forEach(cell => {
        cell.onclick = () => {
            const picked = new Date(cell.dataset.date);
            calendar.gotoDate(picked);
            updateDateLabel(picked);
            renderWeekNav(calendar, getStartOfWeek(picked));
            popup.style.display = 'none';
        };
    });
}

// Zeigt die Wochenleiste im Header an (Mo-So Buttons mit Datum)
export function renderWeekNav(calendar, startDate) {
    const container = document.getElementById('day-buttons');
    container.innerHTML = '';
    for (let i = 0; i < 7; i++) {
        const date = new Date(startDate);
        date.setDate(startDate.getDate() + i);
        const btn = document.createElement('button');
        btn.className = 'day-button';
        btn.innerHTML = `${date.toLocaleDateString('de-DE', { weekday: 'short' })}<br>${String(date.getDate()).padStart(2, '0')}`;
        if (date.toDateString() === calendar.getDate().toDateString()) btn.classList.add('active');
        btn.onclick = () => {
            calendar.changeView('resourceTimeGridDay', date);
            renderWeekNav(calendar, startDate);
        };
        container.appendChild(btn);
    }
}

export function updateDateLabel(date, el) {
    el.textContent = date.toLocaleDateString('de-DE', {
        weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
    });
}
