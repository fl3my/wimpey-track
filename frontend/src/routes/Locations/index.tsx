import { createFileRoute, Link } from "@tanstack/react-router";
import { useGetApiLocations } from "@/api/api-client.gen.ts";
import {
  Button,
  Loader,
  Table,
  Text,
  Pagination,
  Group,
  Title,
  Card,
} from "@mantine/core";
import { useState } from "react";
import { IconPlus } from "@tabler/icons-react";
import { CustomLink } from "@/components/custom-link.tsx";

const PAGE_SIZE = 20;

export enum LocationSortBy {
  Name = 0,
  TripCount = 1,
}

export const Route = createFileRoute("/Locations/")({
  component: RouteComponent,
});

function RouteComponent() {
  const {
    data: locations,
    isLoading,
    isError,
    error,
  } = useGetApiLocations({ sortBy: LocationSortBy.Name });

  // Paging
  const [page, setPage] = useState(1);
  const totalPages = Math.ceil((locations?.length ?? 0) / PAGE_SIZE);
  const paginatedLocations = locations?.slice(
    (page - 1) * PAGE_SIZE,
    page * PAGE_SIZE,
  );

  const totalCount = locations?.length ?? 0;
  const startItem = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const endItem = Math.min(page * PAGE_SIZE, totalCount);

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading locations: {(error as Error).message}</Text>
    );

  return (
    <Card withBorder radius="md">
      <Group justify="space-between" align="center" mb="sm">
        <Title order={2}>Locations</Title>

        <Button
          component={Link}
          to="/locations/new"
          leftSection={<IconPlus size={16} />}
        >
          New location
        </Button>
      </Group>
      {locations?.length === 0 ? (
        <Text>No Purchases found</Text>
      ) : (
        <>
          <Group justify="flex-end">
            <Text size="xs" c="dimmed">
              {startItem}–{endItem} / {totalCount}
            </Text>
            <Pagination
              total={totalPages}
              value={page}
              onChange={setPage}
              withPages={false}
            />
          </Group>

          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Name</Table.Th>
                <Table.Th>Occurrences</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {paginatedLocations?.map((location) => (
                <Table.Tr key={location.id}>
                  <Table.Td>{location.name}</Table.Td>
                  <Table.Td>
                    {location.tripCount == 0 ? "Unused" : location.tripCount}
                  </Table.Td>
                  <Table.Td>
                    <CustomLink
                      size={"sm"}
                      variant={"light"}
                      to={"/Locations/$locationId"}
                      params={{ locationId: location.id!.toString() }}
                    >
                      View
                    </CustomLink>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </>
      )}
    </Card>
  );
}
