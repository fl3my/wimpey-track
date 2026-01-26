import { NavLink } from "@mantine/core";
import { Link } from "@tanstack/react-router";
import {
  IconCash,
  IconFileExport,
  IconHome2,
  IconMap2,
  IconReceipt,
  IconRoute,
  IconZoomQuestion,
} from "@tabler/icons-react";

export function Navigation() {
  return (
    <>
      <NavLink
        component={Link}
        to="/"
        label={"Home"}
        leftSection={<IconHome2 size={16} stroke={1.5} />}
      />
      <NavLink
        component={Link}
        to={"/receipts"}
        label={"Receipts"}
        leftSection={<IconReceipt size={16} stroke={1.5} />}
      />
      <NavLink
        component={Link}
        to="/journeys"
        label={"Journeys"}
        leftSection={<IconRoute size={16} stroke={1.5} />}
      />
      <NavLink
        component={Link}
        to="/purchases"
        label={"Purchases"}
        leftSection={<IconCash size={16} stroke={1.5} />}
      />
      <NavLink
        component={Link}
        to="/locations"
        label={"Locations"}
        leftSection={<IconMap2 size={16} stroke={1.5} />}
      />
      <NavLink
        component={Link}
        to="/reasons"
        label={"Reasons"}
        leftSection={<IconZoomQuestion size={16} stroke={1.5} />}
      />
      <NavLink
        component={Link}
        to="/Reports"
        label={"Reports"}
        leftSection={<IconFileExport size={16} stroke={1.5} />}
      />
    </>
  );
}
