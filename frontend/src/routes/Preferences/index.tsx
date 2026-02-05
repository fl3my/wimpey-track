import { createFileRoute } from "@tanstack/react-router";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import {
  useGetApiPreference,
  usePutApiPreference,
} from "@/api/api-client.gen.ts";
import { useForm } from "@mantine/form";
import { putApiPreferenceBody } from "@/api/zod.gen.ts";
import { zod4Resolver } from "mantine-form-zod-resolver";
import z from "zod";
import { useEffect, useState } from "react";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import { Alert, Button, NumberInput, Title } from "@mantine/core";

export const Route = createFileRoute("/Preferences/")({
  component: RouteComponent,
});

function RouteComponent() {
  const serverErrors = useServerErrors();
  const [saved, setSaved] = useState(false);

  const preferences = useGetApiPreference();

  // Prepopulate the form
  useEffect(() => {
    if (!preferences.data) return;
    form.setValues(preferences.data);
    form.resetDirty(preferences.data);
  }, [preferences.data]);

  const form = useForm<z.infer<typeof putApiPreferenceBody>>({
    initialValues: {
      milesAdjustmentFactor: 1,
    },
    validate: zod4Resolver(putApiPreferenceBody),
  });

  const mutation = usePutApiPreference({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
        setSaved(false);
      },
      onSuccess: async () => {
        serverErrors.clear();
        setSaved(true);
      },
    },
  });

  const handleSubmit = (values: typeof form.values) => {
    setSaved(false);
    form.clearErrors();
    mutation.mutate({
      data: {
        milesAdjustmentFactor: values.milesAdjustmentFactor,
      },
    });
  };

  return (
    <>
      <Title order={2} mb={"md"}>
        Edit Preferences
      </Title>

      <form onSubmit={form.onSubmit(handleSubmit)}>
        <ServerErrorAlert errors={serverErrors.errors} />
        {saved && (
          <Alert color="green" mb="md">
            Preferences saved successfully
          </Alert>
        )}
        <NumberInput
          min={0.5}
          max={2}
          label={"Miles Adjustment Factor"}
          description={
            "This setting is used to adjust the calculated distance between trips"
          }
          key={form.key("milesAdjustmentFactor")}
          {...form.getInputProps("milesAdjustmentFactor")}
        />
        <Button
          type="submit"
          loading={mutation.isPending}
          disabled={mutation.isPending}
        >
          Submit
        </Button>
      </form>
    </>
  );
}
