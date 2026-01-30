import { createFileRoute } from "@tanstack/react-router";
import { useGetPurchasesId } from "@/api/api-client.gen.ts";
import { Loader, Stack, Text } from "@mantine/core";

export const Route = createFileRoute("/Purchases/$purchaseId/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { purchaseId } = Route.useParams();
  const {
    data: purchase,
    isLoading,
    isError,
    error,
  } = useGetPurchasesId(Number(purchaseId));

  if (isLoading) return <Loader size="xl" />;
  if (isError)
    return (
      <Text c="red">Error loading purchase: {(error as Error)?.message}</Text>
    );

  return (
    <Stack>
      <Text size="xl">Purchase</Text>
      <Text>Date: {purchase?.date}</Text>
      <Text>Store: {purchase?.storeName}</Text>

      {purchase?.items?.length ? (
        <Stack>
          <Text>Items:</Text>
          {purchase.items.map((item) => (
            <Text key={item.id}>
              {item.name} — Qty: {item.quantity} — Price: {item.cost}
            </Text>
          ))}
        </Stack>
      ) : (
        <Text>No items for this purchase yet.</Text>
      )}
    </Stack>
  );
}
