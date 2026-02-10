import {
  IconCash,
  IconFileExport,
  IconHome2,
  IconMail,
  IconMap2,
  IconReceipt,
  IconRoute,
  IconSettings,
  IconUser,
  IconZoomQuestion,
} from "@tabler/icons-react";
import { CustomNavLink } from "@/components/custom-navlink.tsx";

export function Navigation() {
  return (
    <>
      <CustomNavLink
        to="/"
        label={"Home"}
        leftSection={<IconHome2 size={16} stroke={1.5} />}
      />
      <CustomNavLink
        to="/Journeys"
        label={"Journeys"}
        leftSection={<IconRoute size={16} stroke={1.5} />}
        search={{ weekStart: "" }}
      />
      <CustomNavLink
        to={"/Receipts"}
        label={"Receipts"}
        leftSection={<IconReceipt size={16} stroke={1.5} />}
      />
      <CustomNavLink
        to="/Purchases"
        label={"Purchases"}
        leftSection={<IconCash size={16} stroke={1.5} />}
      />
      <CustomNavLink
        to="/Locations"
        label={"Locations"}
        leftSection={<IconMap2 size={16} stroke={1.5} />}
      />
      <CustomNavLink
        to="/Reasons"
        label={"Reasons"}
        leftSection={<IconZoomQuestion size={16} stroke={1.5} />}
      />
      <CustomNavLink
        to="/Reports"
        label={"Reports"}
        leftSection={<IconFileExport size={16} stroke={1.5} />}
      />
      <CustomNavLink
        to="/emailrecipients"
        label={"Email Recipients"}
        leftSection={<IconMail size={16} stroke={1.5} />}
      />
      <CustomNavLink
        to="/Profile"
        label={"Profile"}
        leftSection={<IconUser size={16} stroke={1.5} />}
      />
      <CustomNavLink
        to="/Preferences"
        label={"Preferences"}
        leftSection={<IconSettings size={16} stroke={1.5} />}
      />
    </>
  );
}
