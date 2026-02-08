import { createFileRoute } from "@tanstack/react-router";
import { useGetApiReceiptsId } from "@/api/api-client.gen.ts";
import { Loader, Stack, Text, Image, Card, Title } from "@mantine/core";

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

  if (isLoading) return <Loader size="xl" />;
  if (isError)
    return (
      <Text c="red">Error loading receipt: {(error as Error)?.message}</Text>
    );

  return (
    <Card withBorder radius={"md"}>
      <Stack>
        <Title order={2}>View Receipt</Title>
        <Text>{receipt?.date}</Text>
        <Text>{receipt?.name}</Text>
        <Text>{receipt?.category == 0 ? "Purchase" : "Fuel"}</Text>
        <Image alt="text" mah={600} fit={"contain"} src={receipt?.imagePath} />
      </Stack>
    </Card>
  );
}
