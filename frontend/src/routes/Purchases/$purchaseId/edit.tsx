import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/Purchases/$purchaseId/edit')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/Purchases/$purchaseId/edit"!</div>
}
