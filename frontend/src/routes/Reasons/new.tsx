import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useForm } from "@mantine/form";
import { Button, TextInput } from "@mantine/core";
import { zod4Resolver } from "mantine-form-zod-resolver";
import { postApiReasonsBody } from "@/api/zod.gen.ts";
import { usePostApiReasons } from "@/api/api-client.gen.ts";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import z from "zod";

export const Route = createFileRoute("/Reasons/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();

  const serverErrors = useServerErrors();

  const form = useForm<z.infer<typeof postApiReasonsBody>>({
    initialValues: {
      name: "",
    },
    validate: zod4Resolver(postApiReasonsBody),
  });

  const mutation = usePostApiReasons({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        form.reset();
        await navigate({ to: "/Reasons" });
      },
    },
  });

  const handleSubmit = (values: typeof form.values) => {
    form.clearErrors();
    mutation.mutate({
      data: {
        name: values.name,
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
