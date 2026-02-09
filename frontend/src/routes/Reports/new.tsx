import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useForm } from "@mantine/form";
import { MonthPickerInput } from "@mantine/dates";
import { Button, Card, Group, Stack, Text, Title } from "@mantine/core";
import { usePostApiReport } from "@/api/api-client.gen.ts";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";

export const Route = createFileRoute("/Reports/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();
  const serverErrors = useServerErrors();

  const mutation = usePostApiReport({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: async (values) => {
        serverErrors.clear();
        form.reset();
        await navigate({
          to: "/Reports/$reportId",
          params: { reportId: String(values.reportId) },
        });
      },
    },
  });

  const today = new Date();

  const form = useForm({
    mode: "uncontrolled",
    initialValues: {
      startMonth: new Date(today.getFullYear(), today.getMonth() - 1, 1),
    },
    validate: {
      startMonth: (value) => (value ? null : "Start month is required"),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    if (!values.startMonth) return;

    const startMonthDate = new Date(values.startMonth);

    const startDate = new Date(
      startMonthDate.getFullYear(),
      startMonthDate.getMonth(),
      20,
    );

    const endDate = new Date(
      startMonthDate.getFullYear(),
      startMonthDate.getMonth() + 1,
      19,
    );

    const startDateString = formatDateLocal(startDate);
    const endDateString = formatDateLocal(endDate);

    mutation.mutate({
      params: {
        startDate: startDateString,
        endDate: endDateString,
      },
    });
  };

  return (
    <Card>
      <Group justify={"space-between"} mb={"md"}>
        <Title order={3}>Generate Report</Title>
        <CustomButtonLink to={"/Reports"}>Cancel</CustomButtonLink>
      </Group>
      <Text size="sm" c="dimmed" mb={"md"}>
        Select the month you want to generate your expense report for.
      </Text>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <ServerErrorAlert errors={serverErrors.errors} />
        <Stack>
          <MonthPickerInput
            label={"Expenses start month"}
            placeholder={"Expenses start month"}
            key={form.key("startMonth")}
            {...form.getInputProps("startMonth")}
          />
          <Button type="submit" loading={mutation.isPending}>
            Generate
          </Button>
        </Stack>
      </form>
    </Card>
  );
}

// Time zone issues
function formatDateLocal(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0"); // months are 0-indexed
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}
