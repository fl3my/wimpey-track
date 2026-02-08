import { createFileRoute, useNavigate } from "@tanstack/react-router";
import {
  useDeleteApiPurchasesId,
  useGetApiPurchasesId,
} from "@/api/api-client.gen.ts";
import {
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

export const Route = createFileRoute("/Purchases/$purchaseId/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { purchaseId } = Route.useParams();
  const {
    data: purchase,
    isLoading,
    isError,
    error,
  } = useGetApiPurchasesId(Number(purchaseId));
  const serverErrors = useServerErrors();
  const navigate = useNavigate();
  const [opened, { open, close }] = useDisclosure(false);

  const deletePurchase = useDeleteApiPurchasesId({
    mutation: {
      onError: (error) => {
        close();
        serverErrors.setFromApiError(error);
      },
      onSuccess: async () => {
        serverErrors.clear();
        close();
        await navigate({ to: "/Purchases" });
      },
    },
  });

  const handleDelete = () => {
    deletePurchase.mutate({
      id: Number(purchaseId),
    });
  };

  if (isLoading) return <Loader size="xl" />;
  if (isError)
    return (
      <Text c="red">Error loading purchase: {(error as Error)?.message}</Text>
    );

  return (
    <>
      <Modal
        opened={opened}
        onClose={close}
        title="Delete Purchase"
        zIndex={3000}
      >
        <Stack gap="md">
          <Text size="sm">
            Are you sure you want to delete{" "}
            <Text span fw={600}>
              {purchase?.storeName}
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
              loading={deletePurchase.isPending}
              onClick={handleDelete}
            >
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>
      <Card withBorder radius={"md"}>
        <Stack>
          <Group justify={"space-between"}>
            <Title order={3}>Purchase details</Title>
            <Group>
              <CustomButtonLink to={"/Purchases"} variant={"light"}>
                Back
              </CustomButtonLink>
              <Button
                color="red"
                variant="light"
                loading={deletePurchase.isPending}
                onClick={open}
              >
                Delete
              </Button>
            </Group>
          </Group>
          <Text>Date: {purchase?.date}</Text>
          <Text>Store: {purchase?.storeName}</Text>

          {purchase?.items?.length ? (
            <Stack>
              <Text>Items:</Text>
              {purchase.items.map((item) => (
                <Text key={item.id}>
                  {item.name} — Qty: {item.quantity} — Price: {item.cost}
                </Text>
              ))}
            </Stack>
          ) : (
            <Text>No items for this purchase yet.</Text>
          )}
        </Stack>
      </Card>
    </>
  );
}
