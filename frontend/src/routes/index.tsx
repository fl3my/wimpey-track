import { createFileRoute } from "@tanstack/react-router";
import { Card, SimpleGrid, Text, Title } from "@mantine/core";
import { LineChart, BarChart } from "@mantine/charts";

export const Route = createFileRoute("/")({
  component: Home,
});

function Home() {
  const cumulativeMiles = [
    { month: "Apr", miles: 1200 },
    { month: "May", miles: 2600 },
    { month: "Jun", miles: 4000 },
    { month: "Jul", miles: 5500 },
    { month: "Aug", miles: 7000 },
    { month: "Sep", miles: 8500 },
    { month: "Oct", miles: 10000 },
    { month: "Nov", miles: 11500 },
    { month: "Dec", miles: 13000 },
    { month: "Jan", miles: 14500 },
    { month: "Feb", miles: 16000 },
    { month: "Mar", miles: 17500 },
  ];

  const miles = [
    { month: "Apr", miles: 1200 },
    { month: "May", miles: 1600 },
    { month: "Jun", miles: 1000 },
    { month: "Jul", miles: 1500 },
    { month: "Aug", miles: 1000 },
    { month: "Sep", miles: 1500 },
    { month: "Oct", miles: 1000 },
    { month: "Nov", miles: 1500 },
    { month: "Dec", miles: 1000 },
    { month: "Jan", miles: 1500 },
    { month: "Feb", miles: 1000 },
    { month: "Mar", miles: 1500 },
  ];

  return (
    <>
      <Title order={2} mb={"md"}>
        Dashboard
      </Title>

      <SimpleGrid cols={3} spacing="md" mb="lg">
        <Card shadow="sm" p="md">
          <Text w={500}>Total claimed this tax year</Text>
          <Text size="xl" w={700}>
            £TBD
          </Text>
        </Card>

        <Card shadow="sm" p="md">
          <Text w={500}>Total claimed this month</Text>
          <Text size="xl" w={700}>
            £TBD
          </Text>
        </Card>

        <Card shadow="sm" p="md">
          <Text w={500}>Total mileage this tax year</Text>
          <Text size="xl" w={700}>
            TBD miles
          </Text>
        </Card>
      </SimpleGrid>
      <SimpleGrid cols={1} spacing="md">
        <Card shadow="sm" p="md">
          <Text w={500} mb={"sm"}>
            Cumulative Miles This Year
          </Text>
          <LineChart
            h={300}
            data={cumulativeMiles}
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
        <Card shadow="sm" p="md">
          <Text w={500} mb={"sm"}>
            Expenses This Year per Month
          </Text>
          <BarChart
            h={300}
            data={miles}
            dataKey={"month"}
            xAxisLabel="Month"
            yAxisLabel="Miles Travelled"
            series={[{ name: "miles", color: "indigo.6" }]}
          />
        </Card>
      </SimpleGrid>
    </>
  );
}
