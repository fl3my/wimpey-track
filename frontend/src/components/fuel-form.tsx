import { useForm } from "@mantine/form";
import {
  Button,
  Card,
  Group,
  Stack,
  Text,
  TextInput,
  Title,
} from "@mantine/core";
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
    <Card withBorder radius="md">
      <form onSubmit={form.onSubmit(onSubmit)}>
        <Stack>
          <Title order={2}>Fuel Form</Title>
          <Text size="sm" c="dimmed">
            Please check the data extracted from the receipt below.
          </Text>
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
