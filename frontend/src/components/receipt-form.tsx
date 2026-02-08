import { useForm } from "@mantine/form";
import {
  Button,
  Center,
  FileInput,
  Group,
  Image,
  Select,
  Stack,
  TextInput,
} from "@mantine/core";
import { DateInput } from "@mantine/dates";
import { postApiReceiptsBody } from "@/api/zod.gen.ts";
import z from "zod";
import { zod4Resolver } from "mantine-form-zod-resolver";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";

export type ReceiptFormValues = z.infer<typeof postApiReceiptsBody>;

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
      Category: initialValues?.Category,
      File: undefined as File | undefined,
    },
    validate: zod4Resolver(postApiReceiptsBody),
  });
  return (
    <form onSubmit={form.onSubmit(onSubmit)}>
      <Stack gap="md">
        <TextInput
          label={"Name"}
          placeholder={"Name"}
          key={form.key("Name")}
          {...form.getInputProps("Name")}
        />
        <DateInput
          label={"Date"}
          placeholder={"Date"}
          key={form.key("Date")}
          {...form.getInputProps("Date")}
        />
        <Select
          label={"Category"}
          placeholder={"Category"}
          description={"What kind of receipt is it?"}
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
          description={
            "The edges of the receipt should be visible and not distorted. The background should also be dark. "
          }
          clearable
          {...form.getInputProps("File")}
        />
        {form.values.File && (
          <Center>
            <Image
              src={URL.createObjectURL(form.values.File)}
              radius="md"
              mt="sm"
              maw={500}
            />
          </Center>
        )}
        <Group justify={"flex-end"}>
          <CustomButtonLink to={"/Receipts"} variant={"light"}>
            Cancel
          </CustomButtonLink>
          <Button type="submit" loading={isLoading}>
            Submit
          </Button>
        </Group>
      </Stack>
    </form>
  );
}
