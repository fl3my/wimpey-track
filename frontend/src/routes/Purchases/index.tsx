import { createFileRoute } from "@tanstack/react-router";
import { useGetApiPurchases } from "@/api/api-client.gen.ts";
import {
  Card,
  Group,
  Loader,
  Pagination,
  Table,
  Text,
  Title,
} from "@mantine/core";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";
import { CustomLink } from "@/components/custom-link.tsx";
import { useState } from "react";

export const Route = createFileRoute("/Purchases/")({
  component: RouteComponent,
});

const PAGE_SIZE = 20;

function RouteComponent() {
  const { data: purchases, isLoading, isError, error } = useGetApiPurchases();
  const [page, setPage] = useState(1);

  const totalPages = Math.ceil((purchases?.length ?? 0) / PAGE_SIZE);
  const paginatedPurchases = purchases?.slice(
    (page - 1) * PAGE_SIZE,
    page * PAGE_SIZE,
  );

  const totalCount = purchases?.length ?? 0;
  const startItem = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const endItem = Math.min(page * PAGE_SIZE, totalCount);

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading purchases: {(error as Error).message}</Text>
    );

  return (
    <Card withBorder radius={"md"}>
      <Group justify={"space-between"} mb={"md"}>
        <Title order={3}>Purchases</Title>
        <CustomButtonLink to={"/Purchases/new"}>New Purchase</CustomButtonLink>
      </Group>
      <Text size="sm" c="dimmed" mb={"md"}>
        Purchases added here will be added to the generated report.
      </Text>
      {purchases?.length === 0 ? (
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
                <Table.Th>Date</Table.Th>
                <Table.Th>Store Name</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {paginatedPurchases?.map((purchase) => (
                <Table.Tr key={purchase.id}>
                  <Table.Td>{purchase.date}</Table.Td>
                  <Table.Td>{purchase.storeName}</Table.Td>
                  <Table.Td>
                    <CustomLink
                      size={"sm"}
                      to={"/Purchases/$purchaseId"}
                      params={{ purchaseId: purchase.id!.toString() }}
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
