import { createFileRoute, useNavigate } from "@tanstack/react-router";
import {
  useDeleteApiLocationsId,
  useGetApiLocationsId,
} from "@/api/api-client.gen.ts";
import {
  AspectRatio,
  Card,
  Loader,
  Stack,
  Text,
  Title,
  Modal,
  Group,
  Button,
} from "@mantine/core";
import { Marker, MapContainer } from "react-leaflet";
import { PMTilesLayer } from "@/components/pm-tiles-layer.tsx";
import { useDisclosure } from "@mantine/hooks";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";

export const Route = createFileRoute("/Locations/$locationId/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { locationId } = Route.useParams();
  const [opened, { open, close }] = useDisclosure(false);
  const navigate = useNavigate();
  const serverErrors = useServerErrors();

  const {
    data: location,
    isLoading,
    isError,
    error,
  } = useGetApiLocationsId(Number(locationId));

  const deleteLocation = useDeleteApiLocationsId({
    mutation: {
      onError: (error) => {
        close();
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        close();
        await navigate({ to: "/Locations" });
      },
    },
  });

  const handleDelete = () => {
    deleteLocation.mutate({
      id: Number(locationId),
    });
  };

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
    <>
      <Modal
        opened={opened}
        onClose={close}
        title="Delete Location"
        zIndex={3000}
      >
        <Stack gap="md">
          <Text size="sm">
            Are you sure you want to delete{" "}
            <Text span fw={600}>
              {location?.name}
            </Text>
            ?
          </Text>

          <Text size="sm" c="dimmed">
            This action cannot be undone.
          </Text>

          <Group justify="flex-end">
            <Button variant="default" onClick={close}>
              Cancel
            </Button>
            <Button
              color="red"
              loading={deleteLocation.isPending}
              onClick={handleDelete}
            >
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>

      <Stack gap="md">
        <ServerErrorAlert errors={serverErrors.errors} />
        <Card withBorder radius={"md"}>
          <Stack>
            <Group justify="space-between">
              <Title order={3}>Location</Title>

              <Button
                color="red"
                variant="light"
                loading={deleteLocation.isPending}
                onClick={open}
              >
                Delete
              </Button>
            </Group>
            <Text>{location?.name}</Text>
            <AspectRatio ratio={16 / 9} w={"100%"}>
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
        </Card>
      </Stack>
    </>
  );
}
