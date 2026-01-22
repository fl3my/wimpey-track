import { createFileRoute, Link } from "@tanstack/react-router";
import { useDeleteApiReasonsId, useGetApiReasons } from "@/api-client.gen.ts";
import { Button, Loader, Table, Text } from "@mantine/core";

export const Route = createFileRoute("/Reasons/")({
  component: RouteComponent,
});

function RouteComponent() {
  const {
    data: reasons,
    isLoading,
    isError,
    error,
    refetch,
  } = useGetApiReasons();
  const deleteReason = useDeleteApiReasonsId();

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading locations: {(error as Error).message}</Text>
    );

  const handleDelete = async (id: number) => {
    try {
      await deleteReason.mutateAsync({ id });
      await refetch();
    } catch (err) {
      console.error("Failed to delete reason:", err);
    }
  };
  return (
    <>
      <Button component={Link} to={"/reasons/new"}>
        New Reason
      </Button>
      {reasons?.length === 0 ? (
        <Text>No Purchases found</Text>
      ) : (
        <Table>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Name</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {reasons?.map((reason) => (
              <Table.Tr key={reason.id}>
                <Table.Td>{reason.name}</Table.Td>
                <Table.Td>
                  <Button
                    size="xs"
                    color="red"
                    variant="light"
                    onClick={() => handleDelete(Number(reason.id))}
                    loading={deleteReason.isPending}
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
