<template>
  <DayPilotScheduler :config="config" />
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { DayPilot, DayPilotScheduler } from '@daypilot/daypilot-lite-vue'

const config = ref({
  startDate: new DayPilot.Date(),
  days: 1,
  scale: "Hour",
  businessBeginsHour: 8,
  businessEndsHour: 17,
  timeHeaders: [{ groupBy: "Hour" }],
  cellDuration: 60,
  showNonBusiness: false,
  rowHeaderColumns: [{ name: "Mitarbeiter" }],
  resources: [],
  events: [],
  onTimeRangeSelected: args => {
    const name = prompt("Kundenname für Buchung:")
    if (!name) return
    config.value.events.push({
      id: DayPilot.guid(),
      text: name,
      start: args.start,
      end: args.end,
      resource: args.resource
    })
  }
})

onMounted(() => {
  config.value.resources = [
    { name: "Tina", id: "tina" },
    { name: "Peter", id: "peter" },
    { name: "Megan", id: "megan" }
  ]

  config.value.events = [
    {
      id: "1",
      text: "Kunde A",
      start: new DayPilot.Date().addHours(9),
      end: new DayPilot.Date().addHours(10),
      resource: "tina"
    }
  ]
})
</script>

<style>


:root {
  --daypilot-color-header: #1f2937;
  --daypilot-color-background: #111827;
  --daypilot-color-text: white;
  --daypilot-color-grid-lines: #2f2f2f;
  --daypilot-color-event-background: #3b82f6;
  --daypilot-color-event-text: white;
}
</style>
