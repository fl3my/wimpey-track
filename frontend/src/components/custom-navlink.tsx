import * as React from "react";
import { createLink, type LinkComponent } from "@tanstack/react-router";
import { NavLink, type NavLinkProps } from "@mantine/core";

interface MantineNavLinkProps extends Omit<NavLinkProps, "href"> {
  // Add any additional props you want to pass to the anchor
}

const MantineNavLinkComponent = React.forwardRef<
  HTMLAnchorElement,
  MantineNavLinkProps
>((props, ref) => {
  return <NavLink ref={ref} {...props} />;
});

const CreatedNavLinkComponent = createLink(MantineNavLinkComponent);

export const CustomNavLink: LinkComponent<typeof MantineNavLinkComponent> = (
  props,
) => {
  return <CreatedNavLinkComponent preload="intent" {...props} />;
};
