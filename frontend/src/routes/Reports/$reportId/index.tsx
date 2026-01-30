import { createFileRoute } from "@tanstack/react-router";
import { useGetReportId } from "@/api/api-client.gen.ts";
import { Anchor, Group, Stack, Text } from "@mantine/core";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";

export const Route = createFileRoute("/Reports/$reportId/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { reportId } = Route.useParams();
  const { data: preview } = useGetReportId(reportId);

  return (
    <>
      <CustomButtonLink to={"/Reports"}>Back</CustomButtonLink>
      <Stack>
        <Text>Expense Sheets</Text>
        {preview &&
          preview.expenseDocuments?.map((doc) => (
            <Group key={doc.fileName}>
              <Text>{doc.fileName}</Text>
              <Anchor href={doc.url}>Preview</Anchor>
              <Anchor href={doc.url} download>
                Download
              </Anchor>
            </Group>
          ))}
        <Text>Receipts</Text>
        {preview &&
          preview.receiptPages?.map((doc) => (
            <Group key={doc.fileName}>
              <Text>{doc.fileName}</Text>
              <Anchor href={doc.url}>Preview</Anchor>
              <Anchor href={doc.url} download>
                Download
              </Anchor>
            </Group>
          ))}
      </Stack>
    </>
  );
}
