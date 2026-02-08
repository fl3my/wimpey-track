import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { usePostApiReceipts } from "@/api/api-client.gen.ts";
import {
  ReceiptForm,
  type ReceiptFormValues,
} from "@/components/receipt-form.tsx";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { Card, Stack, Text, Title } from "@mantine/core";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";

export const Route = createFileRoute("/Receipts/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();
  const serverErrors = useServerErrors();

  const mutate = usePostApiReceipts({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        await navigate({ to: "/Receipts" });
      },
    },
  });

  const handleSubmit = async (values: ReceiptFormValues) => {
    mutate.mutate({
      data: {
        Name: values.Name,
        Date: values.Date,
        Category: values.Category,
        File: values.File,
      },
    });
  };

  return (
    <Card withBorder radius="md">
      <Stack>
        <Title order={3}>New Manual Receipt</Title>
        <Text size="sm" c="dimmed">
          Create a receipt manually. This is uploaded to the report when an
          expense report is created.
        </Text>
        <ServerErrorAlert errors={serverErrors.errors} />
        <ReceiptForm onSubmit={handleSubmit} />
      </Stack>
    </Card>
  );
}
