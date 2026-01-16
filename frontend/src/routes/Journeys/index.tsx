import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/Journeys/')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/Journeys/"!</div>
}
