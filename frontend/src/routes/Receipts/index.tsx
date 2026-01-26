import { createFileRoute } from "@tanstack/react-router";
import { useGetReceipts } from "@/api-client.gen.ts";
import { Loader, Text } from "@mantine/core";

export const Route = createFileRoute("/Receipts/")({
  component: RouteComponent,
});

function RouteComponent() {
  const { data: receipts, isLoading, isError, error } = useGetReceipts();

  if (isLoading) return <Loader />;
  if (isError)
    return (
      <Text c="red">Error loading reasons: {(error as Error).message}</Text>
    );

  return <div>Hello "/Receipts/"!</div>;
}
