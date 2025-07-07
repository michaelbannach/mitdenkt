// Import von Funktionen aus separaten Modulen
// UI-Funktionen für Kalendarnavigation und Minikalendar (im Header)
import { renderMiniCalendar, renderWeekNav, updateDateLabel } from './calendar-ui.js';

// Hilfsfunktion zum Berechnen des Wochenanfangs
import { getStartOfWeek } from './utils.js';

// Mit dieser Funktion werden alle Buchungen aus der API geholt und als Event formatiert
import { loadCalendarEvents } from './calendar-events.js';

// Öffnet und füllt das Buchungsmodul bei Slot-Auswahl
import { setupBookingModal } from './booking-modal.js';

document.addEventListener('DOMContentLoaded', async function () {
    const calendarEl = document.getElementById('calendar');
    const dateLabel = document.getElementById('date-label');
    
    
// Holt alle Mitarbeiter aus der API
    const resourceRes = await fetch('/api/employee', { credentials: 'include' });
    const resources = await resourceRes.json();
    
    
// Holt alle Dienstleistungen aus der API 
    const serviceRes = await fetch('/api/service', { credentials: 'include' });
    const services = await serviceRes.json();

    
    
    //Zuweisung von Farbe und Bild für Anzeige im Kalendar
    const employees = {
        "Tina": { color: '#ff6f6f', image: '/img/portrait_1.png' },
        "Peter": { color: '#b2d4b2', image: '/img/portrait_2.png' },
        "Sonja": { color: '#aed3e3', image: '/img/portrait_3.png' },
        "Michael": { color: '#ff7f50', image: '/img/portrait_4.png' }
    };

    const calendar = new FullCalendar.Calendar(calendarEl, {
        schedulerLicenseKey: 'CC-Attribution-NonCommercial-NoDerivatives',
        initialView: 'resourceTimeGridDay', //Tagesansicht mit Friseuren
        locale: 'de',
        allDaySlot: false,
        headerToolbar: false,
        slotDuration: '00:15:00', 
        slotLabelInterval: '01:00',
        slotMinTime: '08:00:00', //Öffnungszeit
        slotMaxTime: '18:00:00', //Öffnungszeit
        contentHeight: 'auto',
        slotLabelFormat: { hour: '2-digit', minute: '2-digit', hour12: false },
        selectable: true,
        editable: false,

        
        // Callback in Fullcalendar, hier können die Kalendareinträge individuell angepasst werden
        // Erweiterungen wie gebuchte Dienstleistungen, Name des Kunden möglich
        eventContent: arg => {
            const pad = n => String(n).padStart(2, '0');
            const start = new Date(arg.event.start);
            const end = new Date(arg.event.end);
            return {
                html: `<div style="font-size: 0.6rem">${pad(start.getHours())}:${pad(start.getMinutes())} – ${pad(end.getHours())}:${pad(end.getMinutes())}</div>`
            };
        },

        
        // Funktion formatiert Ressourcen-Zeile je Friseur
        // Aus dem employee-Objekt wird color und image geladen
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
                        <img src="${imgSrc}" alt="${name}" class="resource-portrait" />
                    </div>
                `
            };
        },
        
        
        // Wandelt geladene Ressourcen (z.B. Friseure aus der DB) in ein Format um, das FullCalendar versteht
        resources: resources.map(e => ({ id: e.id, title: e.firstName })),

        events: loadCalendarEvents(employees),

        
        // Wenn ein Benutzer auf ein freies Zeitfenster klickt wird setupBookingModal() aufgerufen
        // Modal-Fenster wird geöffnet
        select: info => setupBookingModal(info, services, calendar),

        
        // Bei Klick auf bereits gebuchten Termin -> Soll Termin wirklich gelöscht werden?
        eventClick: function (info) {
            if (confirm("Diesen Termin löschen?")) {
                fetch(`/api/booking/${info.event.id}`, {
                    method: 'DELETE',
                    credentials: 'include'
                }).then(res => {
                    if (res.ok) calendar.refetchEvents();
                    else res.text().then(msg => alert('Fehler: ' + msg));
                });
            }
        }
    });

    calendar.render();
    
    // Zeigt aktuelles Datum im Header, formatiert und setzt das Label
    updateDateLabel(calendar.getDate(), dateLabel);

    let currentWeekStart = getStartOfWeek(new Date());
    renderWeekNav(calendar, currentWeekStart);

    // Leeres <div> wird erstellt als Container für den Mini-Kalendar und an Body gehängt
    // Wird sichtbar bei Klick auf das Kalendar-Icon
    const trigger = document.getElementById("calendar-trigger");
    const popup = document.createElement('div');
    popup.id = 'mini-calendar-popup';
    popup.className = 'calendar-popup';
    document.body.appendChild(popup);
    
    
    // Mini-Kalendar öffnet bei Klick auf Icon
    // Positionierung des Popup
    trigger.addEventListener('click', (e) => {
        const rect = e.target.getBoundingClientRect();
        popup.style.left = `${rect.left}px`;
        popup.style.top = `${rect.bottom + 5}px`;
        popup.style.display = popup.style.display === 'none' ? 'block' : 'none';
        
        // Zeichnet Mini-Kalendar für aktuellen Monat
        // Bei Datumsauswahl wird updateDateLabel() und renderWeekNav() aufgerufen
        renderMiniCalendar(calendar.getDate(), calendar, (d) => updateDateLabel(d, dateLabel), renderWeekNav);
    });

    // Vorwoche anzeigen
    document.getElementById('prev-week').onclick = () => {
        const prev = new Date(currentWeekStart);
        prev.setDate(prev.getDate() - 7);
        currentWeekStart = prev;
        renderWeekNav(calendar, prev);
        calendar.changeView('resourceTimeGridDay', prev);
    };

    
    
    // Nächste Woche anzeigen
    document.getElementById('next-week').onclick = () => {
        const next = new Date(currentWeekStart);
        next.setDate(next.getDate() + 7);
        currentWeekStart = next;
        renderWeekNav(calendar, next);
        calendar.changeView('resourceTimeGridDay', next);
    };
});
