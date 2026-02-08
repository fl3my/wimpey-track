import { useForm } from "@mantine/form";
import {
  Button,
  Card,
  Group,
  NumberInput,
  Stack,
  Text,
  TextInput,
  Title,
} from "@mantine/core";
import { DateInput } from "@mantine/dates";
import { zod4Resolver } from "mantine-form-zod-resolver";
import { postApiPurchasesBody } from "@/api/zod.gen.ts";
import z from "zod";
import { IconPlus } from "@tabler/icons-react";

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
          reason: "",
        },
      ],
    },
    validate: zod4Resolver(postApiPurchasesBody),
  });

  return (
    <Card>
      <form onSubmit={form.onSubmit(onSubmit)}>
        <Stack>
          <Title order={2}>Purchase Form</Title>
          <Text size="sm" c="dimmed">
            Please check the data extracted from the receipt below. This form
            will automatically create a purchase.
          </Text>
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
            <Card key={index} withBorder>
              <Title order={3} mb={"md"}>
                Item {index + 1}
              </Title>
              <Stack>
                <TextInput
                  label="Item name"
                  placeholder="Item name"
                  key={form.key(`items.${index}.name`)}
                  {...form.getInputProps(`items.${index}.name`)}
                />
                <TextInput
                  label="Reason for purchase"
                  placeholder="Reason for purchase"
                  key={form.key(`items.${index}.reason`)}
                  {...form.getInputProps(`items.${index}.reason`)}
                />
                <Group grow>
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
                </Group>
                <Button
                  color="red"
                  variant="light"
                  disabled={form.values.items.length === 1}
                  onClick={() => form.removeListItem("items", index)}
                >
                  Remove
                </Button>
              </Stack>
            </Card>
          ))}
          <Button
            variant="light"
            leftSection={<IconPlus size={16} />}
            onClick={() =>
              form.insertListItem("items", {
                name: "",
                quantity: 1,
                cost: 0,
                reason: "",
              })
            }
          >
            Add Another Item
          </Button>
          <Group justify={"flex-end"}>
            <Button type="submit" loading={isLoading}>
              Submit
            </Button>
          </Group>
        </Stack>
      </form>
    </Card>
  );
}
