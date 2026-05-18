import { createFileRoute } from "@tanstack/react-router";
import { Button, Card, Group, SimpleGrid, Text, Title } from "@mantine/core";
import { LineChart, BarChart } from "@mantine/charts";
import { useGetApiDashboard } from "@/api/api-client.gen.ts";

export const Route = createFileRoute("/")({
  component: Home,
});

function Home() {
  const { data: dashboard, isLoading } = useGetApiDashboard();

  if (isLoading || !dashboard) return <div>Loading</div>;

  const { summary, monthlyMiles, cumulativeMiles } = dashboard;

  const webcalUrl = `${window.location.origin}/api/cal/calendar.ics`
    .replace("https://", "webcal://")
    .replace("http://", "webcal://");

  return (
    <>
      <Title order={3} mb={"md"}>
        Dashboard
      </Title>

      <Button component="a" href={webcalUrl} mb={"md"}>
        Subscribe to calendar
      </Button>

      <Group grow mb={"md"}>
        <Card shadow="sm" p="md">
          <Text w={500}>Total claimed this tax year</Text>
          <Text size="xl" w={700}>
            £{Number(summary?.totalClaimedThisTaxYear).toFixed(2)}
          </Text>
        </Card>

        <Card shadow="sm" p="md">
          <Text w={500}>Total mileage this tax year</Text>
          <Text size="xl" w={700}>
            {summary?.totalMileageThisTaxYear?.toLocaleString()} miles
          </Text>
        </Card>
      </Group>
      <SimpleGrid cols={1} spacing="md">
        <Card shadow="sm" p="md">
          <Text w={500} mb={"sm"}>
            Total Miles and Claim per Expense Month
          </Text>
          <BarChart
            h={300}
            data={monthlyMiles!}
            dataKey={"month"}
            xAxisLabel="Month"
            yAxisLabel="Miles Travelled"
            series={[
              { name: "miles", color: "indigo.6" },
              { name: "claim", label: "Claim (£)", color: "green.6" },
            ]}
          />
        </Card>
        <Card shadow="sm" p="md">
          <Text w={500} mb={"sm"}>
            Cumulative Miles this Year
          </Text>
          <LineChart
            h={300}
            data={cumulativeMiles!}
            dataKey={"month"}
            xAxisLabel="Month"
            yAxisLabel="Miles Travelled"
            series={[{ name: "miles", color: "indigo.6" }]}
            curveType="linear"
            referenceLines={[
              { y: 10000, label: "Change of Mileage rate", color: "red.6" },
            ]}
          />
        </Card>
      </SimpleGrid>
    </>
  );
}
