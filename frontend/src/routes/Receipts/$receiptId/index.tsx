import { createFileRoute, useNavigate } from "@tanstack/react-router";
import {
  useDeleteApiReceiptsId,
  useGetApiReceiptsId,
} from "@/api/api-client.gen.ts";
import {
  Loader,
  Stack,
  Text,
  Image,
  Card,
  Title,
  Group,
  Button,
  Modal,
} from "@mantine/core";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { useDisclosure } from "@mantine/hooks";

export const Route = createFileRoute("/Receipts/$receiptId/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { receiptId } = Route.useParams();
  const {
    data: receipt,
    isLoading,
    isError,
    error,
  } = useGetApiReceiptsId(Number(receiptId));
  const serverErrors = useServerErrors();
  const navigate = useNavigate();
  const [opened, { open, close }] = useDisclosure(false);

  const deleteReceipt = useDeleteApiReceiptsId({
    mutation: {
      onError: (error) => {
        close();
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        close();
        await navigate({ to: "/Receipts" });
      },
    },
  });

  const handleDelete = () => {
    deleteReceipt.mutate({
      id: Number(receiptId),
    });
  };

  if (isLoading) return <Loader size="xl" />;
  if (isError)
    return (
      <Text c="red">Error loading receipt: {(error as Error)?.message}</Text>
    );

  return (
    <>
      <Modal
        opened={opened}
        onClose={close}
        title="Delete Receipt"
        zIndex={3000}
      >
        <Stack gap="md">
          <Text size="sm">
            Are you sure you want to delete{" "}
            <Text span fw={600}>
              {receipt?.name}
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
              loading={deleteReceipt.isPending}
              onClick={handleDelete}
            >
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>

      <Card withBorder radius={"md"}>
        <Stack>
          <Group justify="space-between">
            <Title order={3}>Receipt</Title>

            <Button
              color="red"
              variant="light"
              loading={deleteReceipt.isPending}
              onClick={open}
            >
              Delete
            </Button>
          </Group>
          <Text>{receipt?.date}</Text>
          <Text>{receipt?.name}</Text>
          <Text>{receipt?.category == 0 ? "Purchase" : "Fuel"}</Text>
          <Image
            alt="text"
            mah={600}
            fit={"contain"}
            src={receipt?.imagePath}
          />
        </Stack>
      </Card>
    </>
  );
}
