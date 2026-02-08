import { createFileRoute } from "@tanstack/react-router";
import { useGetApiReceipts } from "@/api/api-client.gen.ts";
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
import { CustomLink } from "@/components/custom-link";
import { useState } from "react";
import { IconRobot, IconTool } from "@tabler/icons-react";

export const Route = createFileRoute("/Receipts/")({
  component: RouteComponent,
});

const PAGE_SIZE = 20;

function RouteComponent() {
  const { data: receipts, isLoading, isError, error } = useGetApiReceipts();

  // Paging
  const [page, setPage] = useState(1);
  const totalPages = Math.ceil((receipts?.length ?? 0) / PAGE_SIZE);
  const paginatedReceipts = receipts?.slice(
    (page - 1) * PAGE_SIZE,
    page * PAGE_SIZE,
  );

  const totalCount = receipts?.length ?? 0;
  const startItem = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const endItem = Math.min(page * PAGE_SIZE, totalCount);

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading receipts: {(error as Error).message}</Text>
    );

  return (
    <Card withBorder radius="md">
      <Group justify="space-between" align="center" mb="sm">
        <Title order={2}>Receipts</Title>
        <Group>
          <CustomButtonLink
            variant="outline"
            to={"/Receipts/new"}
            leftSection={<IconTool size={16} />}
          >
            New Manual
          </CustomButtonLink>
          <CustomButtonLink
            to={"/Receipts/ocr"}
            leftSection={<IconRobot size={16} />}
          >
            New Automatic
          </CustomButtonLink>
        </Group>
      </Group>
      <Text size="sm" c="dimmed" mb={"md"}>
        Upload images of your expense receipts here. Choose the Automatic option
        to Identify your receipt and automatically create a purchase.
      </Text>
      {receipts?.length === 0 ? (
        <Text>No Receipts found</Text>
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
                <Table.Th>Category</Table.Th>
                <Table.Th>Date</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {paginatedReceipts?.map((receipt) => (
                <Table.Tr key={receipt.id}>
                  <Table.Td>{receipt.name}</Table.Td>
                  <Table.Td>
                    {receipt.category == 0 ? "Purchase" : "Fuel"}
                  </Table.Td>
                  <Table.Td>{receipt.date}</Table.Td>
                  <Table.Td>
                    <CustomLink
                      size={"sm"}
                      to={"/Receipts/$receiptId"}
                      params={{ receiptId: receipt.id!.toString() }}
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
