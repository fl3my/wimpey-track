import { useForm } from "@mantine/form";
import { Button, TextInput } from "@mantine/core";
import { DateInput } from "@mantine/dates";

export type FuelFormValues = {
  name: string;
  date: string;
};

type FuelFormProps = {
  onSubmit: (values: FuelFormValues) => void | Promise<void>;
  isLoading?: boolean;
  initialValues?: Partial<FuelFormValues>;
};

export function FuelForm({
  onSubmit,
  isLoading = false,
  initialValues,
}: FuelFormProps) {
  const form = useForm<FuelFormValues>({
    mode: "uncontrolled",
    initialValues: {
      name: initialValues?.name ?? "",
      date: initialValues?.date ?? "",
    },
  });
  return (
    <form onSubmit={form.onSubmit(onSubmit)}>
      <TextInput
        label={"name"}
        placeholder={"Name"}
        key={form.key("Name")}
        {...form.getInputProps("name")}
      />
      <DateInput
        label={"date"}
        placeholder={"date"}
        key={form.key("date")}
        {...form.getInputProps("date")}
      />
      <Button type="submit" loading={isLoading}>
        Submit
      </Button>
    </form>
  );
}
