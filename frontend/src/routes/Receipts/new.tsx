import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { usePostReceipts } from "@/api/api-client.gen.ts";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";
import {
  ReceiptForm,
  type ReceiptFormValues,
} from "@/components/receipt-form.tsx";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { Stack } from "@mantine/core";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";

export const Route = createFileRoute("/Receipts/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();
  const serverErrors = useServerErrors();

  const mutate = usePostReceipts({
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
    <>
      <CustomButtonLink to={"/Receipts"}>Back</CustomButtonLink>
      <Stack>
        <ServerErrorAlert errors={serverErrors.errors} />
        <ReceiptForm onSubmit={handleSubmit} />
      </Stack>
    </>
  );
}
