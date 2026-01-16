import { createFileRoute } from "@tanstack/react-router";
import { List } from "@mantine/core";

export const Route = createFileRoute("/")({
  component: Home,
});

function Home() {
  return (
    <>
      <List>
        <List.Item>Show total for expense period</List.Item>
        <List.Item>Show total for expense year</List.Item>
        <List.Item>Show money earned</List.Item>
      </List>
    </>
  );
}
