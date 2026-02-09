import { createFileRoute, useNavigate } from "@tanstack/react-router";
import {
  useDeleteApiReportId,
  useGetApiReportId,
} from "@/api/api-client.gen.ts";
import {
  Anchor,
  Button,
  Card,
  Group,
  Loader,
  Modal,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { useDisclosure } from "@mantine/hooks";
import { CreateDraftReportEmailModal } from "@/components/create-draft-report-email-modal.tsx";
import { IconMail } from "@tabler/icons-react";

export const Route = createFileRoute("/Reports/$reportId/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { reportId } = Route.useParams();
  const {
    data: preview,
    isLoading,
    isError,
    error,
  } = useGetApiReportId(reportId);

  const serverErrors = useServerErrors();
  const navigate = useNavigate();
  const [opened, { open, close }] = useDisclosure(false);
  const [draftOpened, { open: openDraft, close: closeDraft }] =
    useDisclosure(false);

  const deleteReport = useDeleteApiReportId({
    mutation: {
      onError: (error) => {
        close();
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        close();
        await navigate({ to: "/Reports" });
      },
    },
  });

  const getName = (startDate: string, endDate: string) => {
    const startDateString = new Date(startDate).toLocaleDateString("en-gb", {
      month: "long",
      day: "numeric",
    });
    const endDateString = new Date(endDate).toLocaleDateString("en-gb", {
      month: "long",
      day: "numeric",
    });
    return `${startDateString} to ${endDateString}`;
  };

  const handleDelete = () => {
    deleteReport.mutate({
      id: reportId,
    });
  };

  if (isLoading) return <Loader size="xl" />;
  if (isError)
    return (
      <Text c="red">Error loading purchase: {(error as Error)?.message}</Text>
    );

  return (
    <>
      <CreateDraftReportEmailModal
        opened={draftOpened}
        onClose={closeDraft}
        reportId={reportId}
      />
      <Modal
        opened={opened}
        onClose={close}
        title="Delete Report"
        zIndex={3000}
      >
        <Stack gap="md">
          <Text size="sm">
            Are you sure you want to delete{" "}
            <Text span fw={600}>
              {preview?.reportId}
            </Text>
            ?
          </Text>

          <Text size="sm" c="dimmed">
            This action cannot be undone.
          </Text>

          <Group justify="flex-end">
            <Button variant="default" onClick={close}>
              Cancel
            </Button>
            <Button
              color="red"
              loading={deleteReport.isPending}
              onClick={handleDelete}
            >
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>
      <Card withBorder radius={"md"}>
        <Group justify="space-between">
          <Title order={3}>Report Detail</Title>
          <Group>
            <CustomButtonLink to={"/Reports"}>Back</CustomButtonLink>
            <Button color={"red"} onClick={open}>
              Delete
            </Button>
          </Group>
        </Group>
        <Stack gap={"lg"}>
          <Text>{getName(preview?.startDate!, preview?.endDate!)}</Text>
          <Stack gap={"sm"}>
            <Title order={4}>Expense Documents</Title>
            {preview?.expenseDocuments?.length ? (
              preview.expenseDocuments?.map((doc) => (
                <Group key={doc.fileName}>
                  <Text>{doc.fileName}</Text>
                  <Anchor href={doc.url}>Preview</Anchor>
                  <Anchor href={doc.url} download>
                    Download
                  </Anchor>
                </Group>
              ))
            ) : (
              <Text c={"dimmed"} size={"sm"}>
                No documents available
              </Text>
            )}
          </Stack>

          <Stack gap={"sm"}>
            <Title order={4}>Receipt pages</Title>
            {preview?.receiptPages?.length ? (
              preview.receiptPages?.map((doc) => (
                <Group key={doc.fileName}>
                  <Text>{doc.fileName}</Text>
                  <Anchor href={doc.url}>Preview</Anchor>
                  <Anchor href={doc.url} download>
                    Download
                  </Anchor>
                </Group>
              ))
            ) : (
              <Text c={"dimmed"} size={"sm"}>
                No Documents available
              </Text>
            )}
          </Stack>
        </Stack>
        <Group justify={"flex-end"} pt={"md"}>
          <Button onClick={openDraft} leftSection={<IconMail size={16} />}>
            Create Gmail Draft
          </Button>
        </Group>
      </Card>
    </>
  );
}
