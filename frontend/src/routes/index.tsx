import { createFileRoute } from "@tanstack/react-router";
import { Card, SimpleGrid, Text, Title } from "@mantine/core";
import { LineChart, BarChart } from "@mantine/charts";
import { useGetApiDashboard } from "@/api/api-client.gen.ts";

export const Route = createFileRoute("/")({
  component: Home,
});

function Home() {
  const { data: dashboard, isLoading } = useGetApiDashboard();

  if (isLoading || !dashboard) return <div>Loading</div>;

  const { summary, monthlyMiles, cumulativeMiles } = dashboard;

  return (
    <>
      <Title order={2} mb={"md"}>
        Dashboard
      </Title>

      <SimpleGrid cols={3} spacing="md" mb="lg">
        <Card shadow="sm" p="md">
          <Text w={500}>Total claimed this month</Text>
          <Text size="xl" w={700}>
            £{Number(summary?.totalClaimedThisMonth).toFixed(2)}
          </Text>
        </Card>

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
      </SimpleGrid>
      <SimpleGrid cols={1} spacing="md">
        <Card shadow="sm" p="md">
          <Text w={500} mb={"sm"}>
            Total Miles per Month
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
