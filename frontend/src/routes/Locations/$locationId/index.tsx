import { createFileRoute } from "@tanstack/react-router";
import { useGetLocationsId } from "@/api-client.gen.ts";
import { Loader, Stack, Text } from "@mantine/core";

export const Route = createFileRoute("/Locations/$locationId/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { locationId } = Route.useParams();
  const {
    data: location,
    isLoading,
    isError,
    error,
  } = useGetLocationsId(locationId);

  if (isLoading) return <Loader size="xl" />;
  if (isError)
    return (
      <Text c="red">Error loading purchase: {(error as Error)?.message}</Text>
    );
  return (
    <Stack>
      <Text size="xl">Location</Text>
      <Text>Name: {location?.name}</Text>
      <Text>Latitude: {location?.latitude}</Text>
      <Text>Longitude: {location?.longitude}</Text>
    </Stack>
  );
}
