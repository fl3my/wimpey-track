import { useForm } from "@mantine/form";
import { Button, FileInput, Select, TextInput } from "@mantine/core";
import { DateInput } from "@mantine/dates";

export type ReceiptFormValues = {
  name: string;
  date: Date | null;
  category: number;
  file: File | null;
};

type ReceiptFormProps = {
  onSubmit: (values: ReceiptFormValues) => void | Promise<void>;
  isLoading?: boolean;
  initialValues?: Partial<ReceiptFormValues>;
};

export function ReceiptForm({
  onSubmit,
  isLoading = false,
  initialValues,
}: ReceiptFormProps) {
  const form = useForm<ReceiptFormValues>({
    mode: "uncontrolled",
    initialValues: {
      name: initialValues?.name ?? "",
      date: initialValues?.date ?? null,
      category: initialValues?.category ?? 0,
      file: null as File | null,
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
      <Select
        label={"category"}
        placeholder={"category"}
        data={[
          { value: "0", label: "Purchase" },
          { value: "1", label: "Fuel" },
        ]}
        key={form.key("category")}
        {...form.getInputProps("category")}
      />
      <FileInput
        label={"Receipt Image Upload"}
        key={form.key("file")}
        accept={"image/jpeg"}
        capture={"environment"}
        placeholder={"Receipt Image Upload"}
        clearable
        {...form.getInputProps("file")}
      />
      <Button type="submit" loading={isLoading}>
        Submit
      </Button>
    </form>
  );
}
