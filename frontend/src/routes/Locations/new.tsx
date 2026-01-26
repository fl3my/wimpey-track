import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useForm } from "@mantine/form";
import { usePostLocations } from "@/api-client.gen.ts";
import { Button, NumberInput, TextInput } from "@mantine/core";

export const Route = createFileRoute("/Locations/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();

  const form = useForm({
    mode: "uncontrolled",
    initialValues: {
      locationName: "",
      latitude: 0,
      longitude: 0,
    },
  });
  const createLocation = usePostLocations();

  const handleSubmit = async (values: typeof form.values) => {
    try {
      console.log(values);
      await createLocation.mutateAsync({
        data: {
          name: values.locationName,
          latitude: values.latitude,
          longitude: values.longitude,
        },
      });

      form.reset();

      await navigate({ to: "/Locations" });
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <TextInput
        label={"Location name"}
        placeholder={"Location Name"}
        key={form.key("locationName")}
        {...form.getInputProps("locationName")}
      />
      <NumberInput
        label={"Latitude"}
        key={form.key("latitude")}
        {...form.getInputProps("latitude")}
      />
      <NumberInput
        label={"Longitude"}
        key={form.key("longitude")}
        {...form.getInputProps("longitude")}
      />
      <Button type="submit" loading={createLocation.isPending}>
        Submit
      </Button>
    </form>
  );
}
