import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { Button, Group, NumberInput, Stack, TextInput } from "@mantine/core";
import { DateInput } from "@mantine/dates";
import { useForm } from "@mantine/form";
import {
  usePostApiPurchases,
  usePostApiPurchasesPurchaseIdItems,
} from "@/api-client.gen.ts";

export const Route = createFileRoute("/Purchases/new")({
  component: RouteComponent,
});

type ItemForm = {
  name: string;
  quantity: number;
  cost: number;
  reason: string;
};

function RouteComponent() {
  const navigate = useNavigate();

  const form = useForm({
    mode: "uncontrolled",
    initialValues: {
      date: null as Date | null,
      storeName: "",
      items: [] as ItemForm[],
    },
  });
  const createPurchase = usePostApiPurchases();
  const createItem = usePostApiPurchasesPurchaseIdItems();

  const handleSubmit = async (values: typeof form.values) => {
    try {
      const purchase = await createPurchase.mutateAsync({
        data: {
          date: values.date?.toString(),
          storeName: values.storeName,
        },
      });

      console.log(purchase);

      for (const item of values.items) {
        createItem.mutate({
          purchaseId: Number(purchase.id),
          data: {
            name: item.name,
            quantity: item.quantity,
            cost: item.cost,
            reason: item.reason,
          },
        });
      }

      form.reset();
      await navigate({ to: "/Purchases" });
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <Stack>
        <DateInput
          label={"Purchase Date"}
          placeholder={"Purchase Date"}
          key={form.key("date")}
          {...form.getInputProps("date")}
        />
        <TextInput
          label={"Store Name"}
          placeholder={"Store Name"}
          key={form.key("storeName")}
          {...form.getInputProps("storeName")}
        />
        {form.values.items.map((_, index) => (
          <Group key={index}>
            <TextInput
              label={"Item name"}
              key={form.key(`items.${index}.name`)}
              {...form.getInputProps(`items.${index}.name`)}
            />
            <NumberInput
              label={"Quantity"}
              min={1}
              key={form.key(`items.${index}.quantity`)}
              {...form.getInputProps(`items.${index}.quantity`)}
            />
            <NumberInput
              label={"Cost"}
              min={0}
              decimalScale={2}
              fixedDecimalScale
              key={form.key(`items.${index}.cost`)}
              {...form.getInputProps(`items.${index}.cost`)}
            />
            <TextInput
              label={"Reason"}
              key={form.key(`items.${index}.reason`)}
              {...form.getInputProps(`items.${index}.reason`)}
            />
            <Button
              color="red"
              variant="light"
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
        <Button type="submit" loading={createPurchase.isPending}>
          Submit
        </Button>
      </Stack>
    </form>
  );
}
