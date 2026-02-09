import { createFileRoute } from "@tanstack/react-router";
import {
  useGetApiEmailRecipients,
  useDeleteApiEmailRecipientsId,
} from "@/api/api-client.gen.ts";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { useState } from "react";
import {
  Anchor,
  Button,
  Card,
  Group,
  Loader,
  Modal,
  Stack,
  Table,
  Text,
  Title,
} from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";
import { IconPlus } from "@tabler/icons-react";

export const Route = createFileRoute("/emailrecipients/")({
  component: RouteComponent,
});

function RouteComponent() {
  const {
    data: recipients,
    isLoading,
    isError,
    error,
    refetch,
  } = useGetApiEmailRecipients();

  const [opened, { open, close }] = useDisclosure(false);

  const serverErrors = useServerErrors();

  const [selectedRecipient, setSelectedRecipient] = useState<{
    id: string;
    name: string;
  } | null>(null);

  const deleteRecipient = useDeleteApiEmailRecipientsId({
    mutation: {
      onError: (error) => {
        close();
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        close();
        setSelectedRecipient(null);
        await refetch();
      },
    },
  });

  const openDeleteModal = (recipient: { id: string; name: string }) => {
    setSelectedRecipient(recipient);
    open();
  };

  const handleConfirmDelete = () => {
    if (!selectedRecipient) return;

    deleteRecipient.mutate({
      id: selectedRecipient.id,
    });
  };

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading locations: {(error as Error).message}</Text>
    );

  return (
    <Stack>
      <Modal
        opened={opened}
        onClose={close}
        title="Delete Email Recipient"
        zIndex={3000}
      >
        <Stack gap="md">
          <Text size="sm">
            Are you sure you want to delete{" "}
            <Text span fw={600}>
              {selectedRecipient?.name}
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
              loading={deleteRecipient.isPending}
              onClick={handleConfirmDelete}
            >
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>

      <ServerErrorAlert errors={serverErrors.errors} />

      <Card withBorder radius="md">
        <Group justify="space-between" align="center" mb="sm">
          <Title order={3}>Email Recipients</Title>
          <CustomButtonLink
            to="/emailrecipients/new"
            leftSection={<IconPlus size={16} />}
          >
            New Email Recipient
          </CustomButtonLink>
        </Group>
        <Text size="sm" c="dimmed" mb={"md"}>
          List of the users your expense reports should be sent to
        </Text>
        {recipients?.length === 0 ? (
          <Text>No Recipients found</Text>
        ) : (
          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Full name</Table.Th>
                <Table.Th>Email</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {recipients?.map((recipient) => (
                <Table.Tr key={recipient.id}>
                  <Table.Td>
                    {recipient.firstName + " " + recipient.lastName}
                  </Table.Td>
                  <Table.Td>{recipient.email}</Table.Td>
                  <Table.Td>
                    <Anchor
                      size={"sm"}
                      c="red"
                      variant="light"
                      onClick={() =>
                        openDeleteModal({
                          id: recipient.id,
                          name: recipient.firstName ?? "",
                        })
                      }
                    >
                      Delete
                    </Anchor>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        )}
      </Card>
    </Stack>
  );
}
