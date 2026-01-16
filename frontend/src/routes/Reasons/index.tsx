import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/Reasons/')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/Reasons/"!</div>
}
