import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useForm } from "@mantine/form";
import { MonthPickerInput } from "@mantine/dates";
import { Button, Group } from "@mantine/core";
import { Alert } from "@mantine/core";
import { usePostReport } from "@/api/api-client.gen.ts";

export const Route = createFileRoute("/Reports/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const { mutateAsync, isPending } = usePostReport();
  const navigate = useNavigate();

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

    try {
      const result = await mutateAsync({
        params: {
          startDate: startDateString,
          endDate: endDateString,
        },
      });

      await navigate({
        to: "/Reports/$reportId",
        params: { reportId: result.reportId! },
      });
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      {form.errors._form && (
        <Alert color="red" mb="md">
          {form.errors._form}
        </Alert>
      )}
      <Group align={"flex-end"}>
        <MonthPickerInput
          label={"Expenses start month"}
          placeholder={"Expenses start month"}
          key={form.key("startMonth")}
          {...form.getInputProps("startMonth")}
        />
        <Button type="submit" loading={isPending}>
          Download
        </Button>
      </Group>
    </form>
  );
}

// Time zone issues
function formatDateLocal(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0"); // months are 0-indexed
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}
