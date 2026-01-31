import { useForm } from "@mantine/form";
import { Button, FileInput, Select, TextInput } from "@mantine/core";
import { DateInput } from "@mantine/dates";
import { postReceiptsBody } from "@/api/zod.gen.ts";
import z from "zod";
import { zod4Resolver } from "mantine-form-zod-resolver";

export type ReceiptFormValues = z.infer<typeof postReceiptsBody>;

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
    initialValues: {
      Name: initialValues?.Name ?? "",
      Date: initialValues?.Date ?? "",
      Category: initialValues?.Category ?? 0,
      File: undefined as File | undefined,
    },
    validate: zod4Resolver(postReceiptsBody),
  });
  return (
    <form onSubmit={form.onSubmit(onSubmit)}>
      <TextInput
        label={"name"}
        placeholder={"Name"}
        key={form.key("Name")}
        {...form.getInputProps("Name")}
      />
      <DateInput
        label={"date"}
        placeholder={"date"}
        key={form.key("Date")}
        {...form.getInputProps("Date")}
      />
      <Select
        label={"category"}
        placeholder={"category"}
        data={[
          { value: "0", label: "Purchase" },
          { value: "1", label: "Fuel" },
        ]}
        key={form.key("Category")}
        {...form.getInputProps("Category")}
      />
      <FileInput
        label={"Receipt Image Upload"}
        key={form.key("File")}
        accept={"image/jpeg"}
        capture={"environment"}
        placeholder={"Receipt Image Upload"}
        clearable
        {...form.getInputProps("File")}
      />
      <Button type="submit" loading={isLoading}>
        Submit
      </Button>
    </form>
  );
}
