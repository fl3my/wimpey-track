import { createFileRoute } from "@tanstack/react-router";
import { useGetApiLocationsId } from "@/api/api-client.gen.ts";
import { AspectRatio, Loader, Stack, Text } from "@mantine/core";
import { Marker, MapContainer } from "react-leaflet";
import { PMTilesLayer } from "@/components/pm-tiles-layer.tsx";

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
  } = useGetApiLocationsId(Number(locationId));

  if (isLoading) return <Loader size="xl" />;
  if (isError)
    return (
      <Text c="red">Error loading purchase: {(error as Error)?.message}</Text>
    );

  if (!location?.latitude || !location?.longitude) {
    return <Text c="red">Invalid location coordinates</Text>;
  }

  const position: [number, number] = [location.latitude, location.longitude];

  return (
    <Stack>
      <Text size="xl">Location</Text>
      <Text>Name: {location?.name}</Text>
      <AspectRatio ratio={16 / 9} w={600}>
        <MapContainer
          center={position}
          zoom={9}
          zoomControl={false}
          dragging={false}
          scrollWheelZoom={false}
        >
          <PMTilesLayer url={`/tiles/scotland-central.pmtiles`} />
          <Marker position={position} />
        </MapContainer>
      </AspectRatio>
    </Stack>
  );
}
