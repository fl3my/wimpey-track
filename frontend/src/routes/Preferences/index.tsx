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
import { useEffect } from "react";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import { Button, NumberInput } from "@mantine/core";

export const Route = createFileRoute("/Preferences/")({
  component: RouteComponent,
});

function RouteComponent() {
  const serverErrors = useServerErrors();

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
      },
      onSuccess: async () => {
        serverErrors.clear();
      },
    },
  });

  const handleSubmit = (values: typeof form.values) => {
    form.clearErrors();
    mutation.mutate({
      data: {
        milesAdjustmentFactor: values.milesAdjustmentFactor,
      },
    });
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <ServerErrorAlert errors={serverErrors.errors} />
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
  );
}
