import { createFileRoute } from "@tanstack/react-router";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";
import { Card, Group, Pagination, Table, Text, Title } from "@mantine/core";
import { useGetApiReport } from "@/api/api-client.gen.ts";
import { useState } from "react";
import { IconRobot } from "@tabler/icons-react";

export const Route = createFileRoute("/Reports/")({
  component: RouteComponent,
});

const PAGE_SIZE = 20;

function RouteComponent() {
  const { data: reports } = useGetApiReport();
  const [page, setPage] = useState(1);

  const totalPages = Math.ceil((reports?.length ?? 0) / PAGE_SIZE);
  const paginatedReports = reports?.slice(
    (page - 1) * PAGE_SIZE,
    page * PAGE_SIZE,
  );

  const totalCount = reports?.length ?? 0;
  const startItem = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const endItem = Math.min(page * PAGE_SIZE, totalCount);

  return (
    <Card withBorder radius="md">
      <Group justify="space-between" mb={"md"}>
        <Title order={3}>Reports</Title>
        <CustomButtonLink
          to={"/Reports/new"}
          leftSection={<IconRobot size={16} />}
        >
          Generate Report
        </CustomButtonLink>
      </Group>
      <Text size="sm" c="dimmed" mb={"md"}>
        Generate a report containing all documents required for your expense
        report. Below are generated expense reports.
      </Text>
      {reports?.length === 0 ? (
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
                <Table.Th>Start Date</Table.Th>
                <Table.Th>End Date</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {paginatedReports?.map((report) => (
                <Table.Tr key={report.id}>
                  <Table.Td>{report.startDate}</Table.Td>
                  <Table.Td>{report.endDate}</Table.Td>
                  <Table.Td>
                    <CustomButtonLink
                      size={"xs"}
                      variant={"light"}
                      to={"/Reports/$reportId"}
                      params={{ reportId: report.id!.toString() }}
                    >
                      View
                    </CustomButtonLink>
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
