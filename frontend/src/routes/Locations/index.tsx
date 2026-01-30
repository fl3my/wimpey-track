import { createFileRoute, Link } from "@tanstack/react-router";
import { useDeleteLocationsId, useGetLocations } from "@/api/api-client.gen.ts";
import { Button, Loader, Table, Text } from "@mantine/core";

export const Route = createFileRoute("/Locations/")({
  component: RouteComponent,
});

function RouteComponent() {
  const {
    data: locations,
    isLoading,
    isError,
    error,
    refetch,
  } = useGetLocations();
  const deleteLocation = useDeleteLocationsId();

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading locations: {(error as Error).message}</Text>
    );

  const handleDelete = async (id: number) => {
    try {
      await deleteLocation.mutateAsync({ id });
      await refetch();
    } catch (err) {
      console.error("Failed to delete location:", err);
    }
  };

  return (
    <>
      <Button component={Link} to={"/locations/new"}>
        New Location
      </Button>
      {locations?.length === 0 ? (
        <Text>No Purchases found</Text>
      ) : (
        <Table>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Name</Table.Th>
              <Table.Th>Latitude</Table.Th>
              <Table.Th>Longitude</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {locations?.map((location) => (
              <Table.Tr key={location.id}>
                <Table.Td>{location.name}</Table.Td>
                <Table.Td>{location.latitude}</Table.Td>
                <Table.Td>{location.longitude}</Table.Td>
                <Table.Td>
                  <Button
                    size={"xs"}
                    variant={"light"}
                    component={Link}
                    to={`/locations/${location.id}`}
                  >
                    View
                  </Button>
                  <Button
                    size="xs"
                    color="red"
                    variant="light"
                    onClick={() => handleDelete(Number(location.id))}
                    loading={deleteLocation.isPending}
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
