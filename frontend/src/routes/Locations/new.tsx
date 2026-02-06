import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useForm } from "@mantine/form";
import { usePostApiLocations } from "@/api/api-client.gen.ts";
import { AspectRatio, Button, TextInput } from "@mantine/core";
import { zod4Resolver } from "mantine-form-zod-resolver";
import { postApiLocationsBody } from "@/api/zod.gen.ts";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import z from "zod";
import { MapContainer } from "react-leaflet";
import { PMTilesLayer } from "@/components/pm-tiles-layer.tsx";
import { ClickableMarker } from "@/components/clickable-marker.tsx";
import type { LatLngBoundsExpression, LatLngTuple } from "leaflet";

export const Route = createFileRoute("/Locations/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();

  const serverErrors = useServerErrors();

  const form = useForm<z.infer<typeof postApiLocationsBody>>({
    initialValues: {
      latitude: 0,
      longitude: 0,
      name: "",
    },
    validate: zod4Resolver(postApiLocationsBody),
  });

  const mutation = usePostApiLocations({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        form.reset();
        await navigate({ to: "/Locations" });
      },
    },
  });

  const maxBounds: LatLngBoundsExpression = [
    [55.3669, -4.9821],
    [56.1059, -2.6953],
  ];

  // derive center from bounds
  const center: LatLngTuple = [
    (maxBounds[0][0] + maxBounds[1][0]) / 2,
    (maxBounds[0][1] + maxBounds[1][1]) / 2,
  ];

  const position: [number, number] | null =
    form.values.latitude && form.values.longitude
      ? [form.values.latitude, form.values.longitude]
      : null;

  const handleSubmit = (values: typeof form.values) => {
    form.clearErrors();
    mutation.mutate({
      data: {
        name: values.name,
        latitude: values.latitude,
        longitude: values.longitude,
      },
    });
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <ServerErrorAlert errors={serverErrors.errors} />
      <TextInput
        label={"Name"}
        placeholder={"Name"}
        key={form.key("name")}
        {...form.getInputProps("name")}
      />
      <AspectRatio ratio={4 / 3} w={600}>
        <MapContainer
          center={center}
          maxBounds={maxBounds}
          zoom={9}
          minZoom={9}
        >
          <PMTilesLayer url={`/tiles/scotland-central.pmtiles`} />
          <ClickableMarker
            position={position}
            onChange={([lat, lng]) => {
              form.setFieldValue("latitude", lat);
              form.setFieldValue("longitude", lng);
            }}
          />
        </MapContainer>
      </AspectRatio>
      <Button
        type="submit"
        loading={mutation.isPending}
        disabled={mutation.isPending}
      >
        Submit
      </Button>
    </form>
  );
}
