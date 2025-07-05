

//Liefert Funktion für FullCalendar damit Events dynamisch geladen werden können
export function loadCalendarEvents(employees) {
    return function (fetchInfo, successCallback, failureCallback) {
        const date = fetchInfo.startStr.split('T')[0];
        fetch('/api/booking/date/' + date, { credentials: 'include' })
            .then(res => res.json())
            .then(data => {
                const events = data.map(b => {
                    const vorname = b.employeeName.split(" ")[0];
                    return {
                        id: b.id,
                        start: b.startTime,
                        end: b.endTime,
                        resourceId: b.employeeId,
                        color: employees[vorname]?.color || '#888'
                    };
                });
                successCallback(events);
            })
            .catch(failureCallback);
    };
}
