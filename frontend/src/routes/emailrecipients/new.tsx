import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { useForm } from "@mantine/form";
import { postApiEmailRecipientsBody } from "@/api/zod.gen.ts";
import { zod4Resolver } from "mantine-form-zod-resolver";
import { usePostApiEmailRecipients } from "@/api/api-client.gen.ts";
import z from "zod";
import {
  Button,
  Card,
  Group,
  Stack,
  Text,
  TextInput,
  Title,
} from "@mantine/core";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";

export const Route = createFileRoute("/emailrecipients/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();

  const serverErrors = useServerErrors();

  const form = useForm<z.infer<typeof postApiEmailRecipientsBody>>({
    initialValues: {
      firstName: "",
      lastName: "",
      email: "",
    },
    validate: zod4Resolver(postApiEmailRecipientsBody),
  });

  const mutation = usePostApiEmailRecipients({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        form.reset();
        await navigate({ to: "/emailrecipients" });
      },
    },
  });

  const handleSubmit = (values: typeof form.values) => {
    form.clearErrors();
    mutation.mutate({
      data: {
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
      },
    });
  };

  return (
    <Card withBorder radius="md">
      <Stack gap="md">
        <Title order={3}>New Email Recipient</Title>
        <Text size="sm" c="dimmed">
          Who would you like to send your expense report to?
        </Text>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap={"md"}>
            <ServerErrorAlert errors={serverErrors.errors} />
            <Group grow>
              <TextInput
                label={"First Name"}
                placeholder={"Last Name"}
                key={form.key("firstName")}
                {...form.getInputProps("firstName")}
              />
              <TextInput
                label={"Last Name"}
                placeholder={"Last Name"}
                key={form.key("lastName")}
                {...form.getInputProps("lastName")}
              />
            </Group>
            <TextInput
              label={"Email Address"}
              placeholder={"Email Address"}
              key={form.key("email")}
              {...form.getInputProps("email")}
            />
            <Group justify={"flex-end"}>
              <Button
                type="submit"
                loading={mutation.isPending}
                disabled={mutation.isPending}
              >
                Submit
              </Button>
            </Group>
          </Stack>
        </form>
      </Stack>
    </Card>
  );
}
