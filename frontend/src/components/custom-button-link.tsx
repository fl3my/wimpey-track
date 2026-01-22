import * as React from "react";
import { createLink, type LinkComponent } from "@tanstack/react-router";
import { Button, type ButtonProps } from "@mantine/core";

interface MantineButtonProps extends Omit<ButtonProps, "component" | "href"> {}

const MantineButtonLinkComponent = React.forwardRef<
  HTMLButtonElement,
  MantineButtonProps
>((props, ref) => {
  return <Button ref={ref} {...props} />;
});

const CreatedButtonLink = createLink(MantineButtonLinkComponent);

export const CustomButtonLink: LinkComponent<
  typeof MantineButtonLinkComponent
> = (props) => {
  return <CreatedButtonLink preload="intent" {...props} />;
};
