import { createFileRoute, Link } from "@tanstack/react-router";
import { useDeletePurchasesId, useGetPurchases } from "@/api/api-client.gen.ts";
import { Button, Loader, Table, Text } from "@mantine/core";

export const Route = createFileRoute("/Purchases/")({
  component: RouteComponent,
});

function RouteComponent() {
  const {
    data: purchases,
    isLoading,
    isError,
    error,
    refetch,
  } = useGetPurchases();
  const deletePurchase = useDeletePurchasesId();

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading purchases: {(error as Error).message}</Text>
    );

  const handleDelete = async (id: number) => {
    try {
      await deletePurchase.mutateAsync({ id });
      await refetch();
    } catch (err) {
      console.error("Failed to delete purchase:", err);
    }
  };

  return (
    <>
      <Button component={Link} to={"/purchases/new"}>
        New Purchase
      </Button>
      {purchases?.length === 0 ? (
        <Text>No Purchases found</Text>
      ) : (
        <Table>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Date</Table.Th>
              <Table.Th>Store Name</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {purchases?.map((purchase) => (
              <Table.Tr key={purchase.id}>
                <Table.Td>{purchase.date}</Table.Td>
                <Table.Td>{purchase.storeName}</Table.Td>
                <Table.Td>
                  <Button
                    size={"xs"}
                    variant={"light"}
                    component={Link}
                    to={`/purchases/${purchase.id}`}
                  >
                    View
                  </Button>
                  <Button
                    size="xs"
                    color="red"
                    variant="light"
                    onClick={() => handleDelete(Number(purchase.id))}
                    loading={deletePurchase.isPending}
                  >
                    Delete
                  </Button>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}
    </>
  );
}
