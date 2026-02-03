import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { usePostApiPurchases } from "@/api/api-client.gen.ts";
import {
  PurchaseForm,
  type PurchaseFormValues,
} from "@/components/purchase-form.tsx";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";

export const Route = createFileRoute("/Purchases/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();

  const serverErrors = useServerErrors();

  const mutate = usePostApiPurchases({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        await navigate({ to: "/Purchases" });
      },
    },
  });

  const handleSubmit = (values: PurchaseFormValues) => {
    mutate.mutate({
      data: {
        date: values.date!.toString(),
        storeName: values.storeName,
        items: values.items,
      },
    });
  };

  return (
    <>
      <ServerErrorAlert errors={serverErrors.errors} />
      <PurchaseForm onSubmit={handleSubmit} isLoading={mutate.isPending} />
    </>
  );
}
