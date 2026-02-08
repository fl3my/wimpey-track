import { createFileRoute } from "@tanstack/react-router";
import {
  useDeleteApiReasonsId,
  useGetApiReasons,
} from "@/api/api-client.gen.ts";
import {
  Anchor,
  Button,
  Card,
  Group,
  Loader,
  Modal,
  Pagination,
  Stack,
  Table,
  Text,
  Title,
} from "@mantine/core";
import { IconPlus } from "@tabler/icons-react";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";
import { useState } from "react";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { useDisclosure } from "@mantine/hooks";

export const Route = createFileRoute("/Reasons/")({
  component: RouteComponent,
});

const PAGE_SIZE = 20;

function RouteComponent() {
  const {
    data: reasons,
    isLoading,
    isError,
    error,
    refetch,
  } = useGetApiReasons();
  const [opened, { open, close }] = useDisclosure(false);

  const serverErrors = useServerErrors();
  const [selectedReason, setSelectedReason] = useState<{
    id: number;
    name: string;
  } | null>(null);

  const [page, setPage] = useState(1);
  const totalPages = Math.ceil((reasons?.length ?? 0) / PAGE_SIZE);
  const paginatedReasons = reasons?.slice(
    (page - 1) * PAGE_SIZE,
    page * PAGE_SIZE,
  );

  const deleteReason = useDeleteApiReasonsId({
    mutation: {
      onError: (error) => {
        close();
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        close();
        setSelectedReason(null);
        await refetch();
      },
    },
  });

  const totalCount = reasons?.length ?? 0;
  const startItem = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const endItem = Math.min(page * PAGE_SIZE, totalCount);

  const openDeleteModal = (reason: { id: number; name: string }) => {
    setSelectedReason(reason);
    open();
  };

  const handleConfirmDelete = () => {
    if (!selectedReason) return;

    deleteReason.mutate({
      id: selectedReason.id,
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
        title="Delete Reason"
        zIndex={3000}
      >
        <Stack gap="md">
          <Text size="sm">
            Are you sure you want to delete{" "}
            <Text span fw={600}>
              {selectedReason?.name}
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
              loading={deleteReason.isPending}
              onClick={handleConfirmDelete}
            >
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>

      <ServerErrorAlert errors={serverErrors.errors} />

      <Card>
        <Group justify="space-between" align="center" mb="sm">
          <Title order={2}>Reasons</Title>
          <CustomButtonLink
            to="/Reasons/new"
            leftSection={<IconPlus size={16} />}
          >
            New Reason
          </CustomButtonLink>
        </Group>
        <Text size="sm" c="dimmed" mb={"md"}>
          These Reasons will show as options in the Journey creation page.
        </Text>
        {reasons?.length === 0 ? (
          <Text>No Reasons found</Text>
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
                  <Table.Th>Occurences</Table.Th>
                  <Table.Th>Actions</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {paginatedReasons?.map((reason) => (
                  <Table.Tr key={reason.id}>
                    <Table.Td>{reason.name}</Table.Td>
                    <Table.Td>
                      {reason.tripCount == 0 ? "Unused" : reason.tripCount}
                    </Table.Td>
                    <Table.Td>
                      <Anchor
                        size={"sm"}
                        c="red"
                        variant="light"
                        onClick={() =>
                          openDeleteModal({
                            id: Number(reason.id),
                            name: reason.name ?? "",
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
          </>
        )}
      </Card>
    </Stack>
  );
}
