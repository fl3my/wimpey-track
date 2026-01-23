import { createFileRoute } from "@tanstack/react-router";
import { useForm } from "@mantine/form";
import { MonthPickerInput } from "@mantine/dates";
import { Button, Group } from "@mantine/core";
import { Alert } from "@mantine/core";
import { useState } from "react";

export const Route = createFileRoute("/Reports/")({
  component: RouteComponent,
});

function RouteComponent() {
  const [downloading, setDownloading] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);

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

    const getMonthName = (date: Date) => {
      return date.toLocaleDateString("default", { month: "short" });
    };

    const fileName = `${getMonthName(startDate)}${getMonthName(endDate)}${startDate.getFullYear()}_Expenses.zip`;

    const startDateString = formatDateLocal(startDate);
    const endDateString = formatDateLocal(endDate);

    try {
      setDownloading(true);
      setSuccess(null);
      form.clearErrors();

      const response = await fetch(
        `/api/report?startDate=${startDateString}&endDate=${endDateString}`,
        {
          method: "GET",
        },
      );

      if (!response.ok) {
        try {
          // Try to parse JSON from the response
          const errorData = await response.json();
          const message = errorData?.message || "Failed to download report";
          form.setErrors({ _form: message });
        } catch {
          // Fallback if response is not JSON
          form.setErrors({ _form: "Failed to download report" });
        }
        return;
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);

      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      a.click();
      window.URL.revokeObjectURL(url);

      setSuccess("Report downloaded successfully");
    } catch {
      form.setErrors({
        _form: "Network error while downloading report",
      });
    } finally {
      setDownloading(false);
    }
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      {form.errors._form && (
        <Alert color="red" mb="md">
          {form.errors._form}
        </Alert>
      )}
      {success && (
        <Alert color="green" mb="md">
          {success}
        </Alert>
      )}
      <Group align={"flex-end"}>
        <MonthPickerInput
          label={"Expenses start month"}
          placeholder={"Expenses start month"}
          key={form.key("startMonth")}
          {...form.getInputProps("startMonth")}
        />
        <Button type="submit" loading={downloading}>
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
