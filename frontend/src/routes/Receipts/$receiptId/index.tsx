import { createFileRoute } from "@tanstack/react-router";
import { useGetReceiptsId } from "@/api-client.gen.ts";
import { Loader, Stack, Text, Image } from "@mantine/core";

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
  } = useGetReceiptsId(receiptId);

  if (isLoading) return <Loader size="xl" />;
  if (isError)
    return (
      <Text c="red">Error loading receipt: {(error as Error)?.message}</Text>
    );

  return (
    <Stack>
      <Text size="xl">Receipt</Text>
      <Text>Name: {receipt?.name}</Text>
      <Text>Date: {receipt?.date}</Text>
      <Text>Category: {receipt?.category}</Text>
      <Image alt="text" h={400} fit={"contain"} src={receipt?.imagePath} />
    </Stack>
  );
}
