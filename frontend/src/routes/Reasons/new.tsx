import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useForm } from "@mantine/form";
import { usePostApiReasons } from "@/api-client.gen.ts";
import { Button, TextInput } from "@mantine/core";

export const Route = createFileRoute("/Reasons/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();

  const form = useForm({
    mode: "uncontrolled",
    initialValues: {
      reasonName: "",
    },
  });
  const createReason = usePostApiReasons();

  const handleSubmit = async (values: typeof form.values) => {
    try {
      console.log(values);
      await createReason.mutateAsync({
        data: {
          name: values.reasonName,
        },
      });

      form.reset();

      await navigate({ to: "/Reasons" });
    } catch (error) {
      console.error(error);
    }
  };
  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <TextInput
        label={"Reason Name"}
        placeholder={"Reason Name"}
        key={form.key("reasonName")}
        {...form.getInputProps("reasonName")}
      />
      <Button type="submit" loading={createReason.isPending}>
        Submit
      </Button>
    </form>
  );
}
