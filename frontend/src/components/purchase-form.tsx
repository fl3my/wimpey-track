import { useForm } from "@mantine/form";
import { Button, Group, NumberInput, Stack, TextInput } from "@mantine/core";
import { DateInput } from "@mantine/dates";
import { zod4Resolver } from "mantine-form-zod-resolver";
import { postApiPurchasesBody } from "@/api/zod.gen.ts";
import z from "zod";

export type PurchaseFormValues = z.infer<typeof postApiPurchasesBody>;

type PurchaseFormProps = {
  onSubmit: (values: PurchaseFormValues) => void | Promise<void>;
  isLoading?: boolean;
  initialValues?: Partial<PurchaseFormValues>;
};

export function PurchaseForm({
  onSubmit,
  isLoading = false,
  initialValues,
}: PurchaseFormProps) {
  const form = useForm<PurchaseFormValues>({
    mode: "uncontrolled",
    initialValues: {
      date: initialValues?.date ?? "",
      storeName: initialValues?.storeName ?? "",
      items: initialValues?.items ?? [
        {
          name: "",
          quantity: 1,
          cost: 0,
          reason: "For plastic surgery",
        },
      ],
    },
    validate: zod4Resolver(postApiPurchasesBody),
  });

  return (
    <form onSubmit={form.onSubmit(onSubmit)}>
      <Stack>
        <DateInput
          label="Purchase Date"
          placeholder="Purchase Date"
          key={form.key("date")}
          {...form.getInputProps("date")}
        />
        <TextInput
          label="Store Name"
          placeholder="Store Name"
          key={form.key("storeName")}
          {...form.getInputProps("storeName")}
        />
        {form.values.items.map((_, index) => (
          <Group key={index}>
            <TextInput
              label="Item name"
              placeholder="Item name"
              key={form.key(`items.${index}.name`)}
              {...form.getInputProps(`items.${index}.name`)}
            />
            <NumberInput
              label="Quantity"
              min={1}
              key={form.key(`items.${index}.quantity`)}
              {...form.getInputProps(`items.${index}.quantity`)}
            />
            <NumberInput
              label="Cost"
              min={0}
              decimalScale={2}
              fixedDecimalScale
              key={form.key(`items.${index}.cost`)}
              {...form.getInputProps(`items.${index}.cost`)}
            />
            <TextInput
              label="Reason"
              key={form.key(`items.${index}.reason`)}
              {...form.getInputProps(`items.${index}.reason`)}
            />
            <Button
              color="red"
              variant="light"
              disabled={form.values.items.length === 1}
              onClick={() => form.removeListItem("items", index)}
            >
              Remove
            </Button>
          </Group>
        ))}
        <Button
          variant="light"
          onClick={() =>
            form.insertListItem("items", {
              name: "",
              quantity: 1,
              cost: 0,
              reason: "For plastic surgery",
            })
          }
        >
          Add Item
        </Button>
        <Button type="submit" loading={isLoading}>
          Submit
        </Button>
      </Stack>
    </form>
  );
}
