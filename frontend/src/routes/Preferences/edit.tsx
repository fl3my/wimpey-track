import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/Preferences/edit')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/Preferences/edit"!</div>
}
