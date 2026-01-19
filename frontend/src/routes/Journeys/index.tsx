import { createFileRoute } from "@tanstack/react-router";
import { usePostApiReasons } from "@/api-client.gen.ts";

export const Route = createFileRoute("/Journeys/")({
  component: RouteComponent,
});

function RouteComponent() {
  const mutation = usePostApiReasons();
  return (
    <>{journeys && journeys.map((w) => <div id={w.name}>{w.name}</div>)}</>
  );
}
