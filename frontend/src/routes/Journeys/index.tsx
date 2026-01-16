import { createFileRoute } from "@tanstack/react-router";
import { useGetWeatherForecast } from "@/api-client.gen.ts";

export const Route = createFileRoute("/Journeys/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { data: weather } = useGetWeatherForecast();
  return (
    <>{weather && weather.map((w) => <div id={w.summary!}>{w.summary}</div>)}</>
  );
}
