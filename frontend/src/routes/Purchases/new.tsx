import { createFileRoute, useNavigate } from "@tanstack/react-router";

import { usePostApiPurchases } from "@/api-client.gen.ts";
import {
  PurchaseForm,
  type PurchaseFormValues,
} from "@/components/purchase-form.tsx";

export const Route = createFileRoute("/Purchases/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();
  const createPurchase = usePostApiPurchases();

  const handleSubmit = async (values: PurchaseFormValues) => {
    try {
      await createPurchase.mutateAsync({
        data: {
          date: values.date?.toString(),
          storeName: values.storeName,
          items: values.items,
        },
      });

      await navigate({ to: "/Purchases" });
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <PurchaseForm
      onSubmit={handleSubmit}
      isLoading={createPurchase.isPending}
    />
  );
}
