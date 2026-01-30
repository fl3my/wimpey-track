import { Alert, List } from "@mantine/core";
import { IconAlertCircle } from "@tabler/icons-react";

type Props = {
  errors: string[];
};

export function ServerErrorAlert({ errors }: Props) {
  if (errors.length === 0) return null;

  return (
    <Alert
      title="Server Errors"
      color="red"
      radius="md"
      icon={<IconAlertCircle size={16} />}
    >
      <List spacing={"xs"} size={"sm"}>
        {errors.map((err, i) => (
          <List.Item key={i}>{err}</List.Item>
        ))}
      </List>
    </Alert>
  );
}
