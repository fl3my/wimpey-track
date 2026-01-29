import { createFileRoute } from "@tanstack/react-router";
import { useGetReportId } from "@/api-client.gen.ts";
import { Anchor, Group, Stack, Text } from "@mantine/core";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";

export const Route = createFileRoute("/Reports/$reportId/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { reportId } = Route.useParams();
  const { data: preview } = useGetReportId(reportId);

  if (preview) {
    console.log(preview);
  }
  return (
    <>
      <CustomButtonLink to={"/Reports"}>Back</CustomButtonLink>
      <Stack>
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
        {preview &&
          preview.receiptPages?.map((doc) => (
            <Anchor key={doc.fileName} href={doc.url}>
              {doc.fileName}
            </Anchor>
          ))}
      </Stack>
    </>
  );
}
