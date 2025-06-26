<template>
  <DayPilotScheduler :config="config" style="height: 600px;" />
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { DayPilotScheduler } from '@daypilot/daypilot-lite-vue'
import axios from '../axios'

const config = ref({
  startDate: DayPilot.Date.today(),
  days: 1,
  scale: "Hour",
  businessBeginsHour: 8,
  businessEndsHour: 18,
  timeHeaders: [{ groupBy: "Hour" }],
  resources: [],
  events: [],
  rowHeaderColumns: [{ text: "Mitarbeiter" }]
})

onMounted(async () => {
  const [empRes, bookRes] = await Promise.all([
    axios.get('/employees'),
    axios.get('/bookings')
  ])

  config.value.resources = empRes.data.map(e => ({
    id: e.id,
    name: `${e.firstName} ${e.lastName}`
  }))

  config.value.events = bookRes.data.map(b => ({
    id: b.id,
    text: `Kunde ${b.customerId}`,
    start: b.startTime,
    end: b.endTime,
    resource: b.employeeId
  }))
})
</script>

<style scoped>
/* optional: z.B. Platz für eigenes Styling */
</style>
