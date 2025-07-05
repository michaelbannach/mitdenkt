
// Funktion berechnet Montag der Woche in der ein bestimmtes Datum liegt
// vorteilhaft um Wochenansicht im Kalendar korrekt zu steuern, aktive Woche zu markieren, Buttons(Mo-So) im Header anzuzeigen
export function getStartOfWeek(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - ((day + 6) % 7);
    return new Date(d.setDate(diff));
}
