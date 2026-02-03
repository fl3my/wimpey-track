import { createFileRoute } from "@tanstack/react-router";
import {
  useDeleteApiReceiptsId,
  useGetApiReceipts,
} from "@/api/api-client.gen.ts";
import { Button, Group, Loader, Table, Text } from "@mantine/core";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";

export const Route = createFileRoute("/Receipts/")({
  component: RouteComponent,
});

function RouteComponent() {
  const {
    data: receipts,
    isLoading,
    isError,
    error,
    refetch,
  } = useGetApiReceipts();
  const deleteReceipt = useDeleteApiReceiptsId();

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading receipts: {(error as Error).message}</Text>
    );

  const handleDelete = async (id: number) => {
    try {
      await deleteReceipt.mutateAsync({ id });
      await refetch();
    } catch (err) {
      console.error("Failed to delete reason:", err);
    }
  };

  return (
    <>
      <Group>
        <CustomButtonLink to={"/Receipts/new"}>
          New Manual Receipt
        </CustomButtonLink>
        <CustomButtonLink to={"/Receipts/ocr"}>
          New Automatic receipt
        </CustomButtonLink>
      </Group>
      {receipts?.length === 0 ? (
        <Text>No Receipts found</Text>
      ) : (
        <Table>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Name</Table.Th>
              <Table.Th>Date</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {receipts?.map((receipt) => (
              <Table.Tr key={receipt.id}>
                <Table.Td>{receipt.name}</Table.Td>
                <Table.Td>{receipt.date}</Table.Td>
                <Table.Td>
                  <CustomButtonLink
                    size={"xs"}
                    variant={"light"}
                    to={"/Receipts/$receiptId"}
                    params={{ receiptId: receipt.id!.toString() }}
                  >
                    View
                  </CustomButtonLink>
                  <Button
                    size="xs"
                    color="red"
                    variant="light"
                    onClick={() => handleDelete(Number(receipt.id))}
                    loading={deleteReceipt.isPending}
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
