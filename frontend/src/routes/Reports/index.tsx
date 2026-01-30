import { createFileRoute } from "@tanstack/react-router";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";
import { Button, Group, Table, Text } from "@mantine/core";
import { useDeleteReportId, useGetReport } from "@/api/api-client.gen.ts";

export const Route = createFileRoute("/Reports/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { data: reports, refetch } = useGetReport();
  const deleteReport = useDeleteReportId();

  const handleDelete = async (id: string) => {
    try {
      await deleteReport.mutateAsync({ id });
      await refetch();
    } catch (err) {
      console.error("Failed to delete report:", err);
    }
  };

  return (
    <>
      <Group>
        <CustomButtonLink to={"/Reports/new"}>Generate Report</CustomButtonLink>
      </Group>
      {reports?.length === 0 ? (
        <Text>No Receipts found</Text>
      ) : (
        <Table>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Start Date</Table.Th>
              <Table.Th>End Date</Table.Th>
              <Table.Th>Created Date</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {reports?.map((report) => (
              <Table.Tr key={report.id}>
                <Table.Td>{report.startDate}</Table.Td>
                <Table.Td>{report.endDate}</Table.Td>
                <Table.Td>{report.generatedAtUtc}</Table.Td>
                <Table.Td>
                  <CustomButtonLink
                    size={"xs"}
                    variant={"light"}
                    to={"/Reports/$reportId"}
                    params={{ reportId: report.id!.toString() }}
                  >
                    View
                  </CustomButtonLink>
                  <Button
                    color={"red"}
                    size={"xs"}
                    onClick={() => handleDelete(report.id!.toString())}
                    loading={deleteReport.isPending}
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
