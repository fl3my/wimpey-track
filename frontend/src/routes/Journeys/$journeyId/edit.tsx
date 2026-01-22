import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useGetApiJourneysId, usePutApiJourneysId } from "@/api-client.gen.ts";
import { useForm } from "@mantine/form";
import {
  Alert,
  Button,
  Center,
  Checkbox,
  Loader,
  NumberInput,
  Stack,
} from "@mantine/core";
import { DateInput } from "@mantine/dates";
import { useEffect } from "react";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";

export const Route = createFileRoute("/Journeys/$journeyId/edit")({
  component: RouteComponent,
  validateSearch: (search: { weekStart?: string }) => ({
    weekStart: search.weekStart,
  }),
});

function RouteComponent() {
  const { journeyId } = Route.useParams();
  const { weekStart } = Route.useSearch();

  const navigate = useNavigate();

  const {
    data: journey,
    isLoading,
    isError,
  } = useGetApiJourneysId(Number(journeyId));

  const form = useForm({
    mode: "uncontrolled",
    initialValues: {
      date: journey?.date,
      totalMiles: journey?.totalMiles ?? 0,
      isManualMiles: journey?.isManualMiles ?? false,
      homeLocationId: journey?.homeLocationId,
    },
  });

  useEffect(() => {
    if (!journey) return;

    form.setValues({
      date: journey.date ?? "",
      totalMiles: journey.totalMiles,
      isManualMiles: journey.isManualMiles,
      homeLocationId: journey.homeLocationId,
    });
  }, [journey]);

  const updateJourney = usePutApiJourneysId();

  const handleSubmit = async (values: typeof form.values) => {
    try {
      await updateJourney.mutateAsync({
        id: Number(journeyId),
        data: {
          id: journeyId,
          date: values.date!,
          totalMiles: values.totalMiles,
          isManualMiles: values.isManualMiles,
          homeLocationId: Number(values.homeLocationId),
        },
      });

      await navigate({ to: "/Journeys", search: { weekStart } });
    } catch (error) {
      console.error(error);
    }
  };

  if (isLoading) {
    return (
      <Center>
        <Loader />
      </Center>
    );
  }

  if (isError) {
    return (
      <Alert color="red" title="Error">
        Failed to load location
      </Alert>
    );
  }

  return (
    <>
      <CustomButtonLink to={"/Journeys"} search={{ weekStart: weekStart }}>
        Back
      </CustomButtonLink>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <DateInput
            label="Date"
            placeholder="Date"
            key={form.key("date")}
            {...form.getInputProps("date")}
          />
          <NumberInput
            label="Total Miles"
            key={form.key("totalMiles")}
            {...form.getInputProps("totalMiles")}
          />
          <Checkbox
            label="Override calculated miles?"
            key={form.key("isManualMiles")}
            {...form.getInputProps("isManualMiles", { type: "checkbox" })}
          />
          <NumberInput
            label="Home location Id"
            key={form.key("homeLocationId")}
            {...form.getInputProps("homeLocationId")}
          />
          <Button type="submit" loading={updateJourney.isPending}>
            Update Journey
          </Button>
        </Stack>
      </form>
    </>
  );
}
