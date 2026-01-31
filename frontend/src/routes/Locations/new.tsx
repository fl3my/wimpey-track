import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useForm } from "@mantine/form";
import { usePostLocations } from "@/api/api-client.gen.ts";
import { Button, NumberInput, TextInput } from "@mantine/core";
import { zod4Resolver } from "mantine-form-zod-resolver";
import { postLocationsBody } from "@/api/zod.gen.ts";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import z from "zod";

export const Route = createFileRoute("/Locations/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();

  const serverErrors = useServerErrors();

  const form = useForm<z.infer<typeof postLocationsBody>>({
    initialValues: {
      latitude: 0,
      longitude: 0,
      name: "",
    },
    validate: zod4Resolver(postLocationsBody),
  });

  const mutation = usePostLocations({
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
