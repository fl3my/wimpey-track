import { createFileRoute, useNavigate } from "@tanstack/react-router";
import {
  useDeleteApiJourneysId,
  useDeleteApiJourneysJourneyIdTripsTripId,
  useGetApiJourneys,
  useGetApiLocations,
  useGetApiReasons,
  usePostApiJourneys,
  usePostApiJourneysJourneyIdTrips,
} from "@/api-client.gen.ts";
import { useState } from "react";
import {
  Button,
  Group,
  Text,
  Center,
  Loader,
  Alert,
  Stack,
  Paper,
  Timeline,
  Select,
  Anchor,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";

export const Route = createFileRoute("/Journeys/")({
  component: RouteComponent,
  validateSearch: (search: { weekStart?: string }) => {
    return {
      weekStart: search.weekStart,
    };
  },
});

function RouteComponent() {
  const { weekStart: weekStartParam } = Route.useSearch();

  // Get the date of monday for any day of the week
  const getMonday = (date: Date) => {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);
    return new Date(d.setDate(diff));
  };

  // Remove the time information from the date
  const formatDate = (date: Date) => {
    return date.toISOString().split("T")[0];
  };

  // Use state to keep track of which week
  const [weekStart, setWeekStart] = useState(() => {
    if (!weekStartParam) return getMonday(new Date());
    return new Date(weekStartParam);
  });

  const [showFormForDay, setShowFormForDay] = useState<number | null>(null);

  // Query the journeys
  const journeys = useGetApiJourneys({
    weekStart: formatDate(weekStart),
  });
  const createJourney = usePostApiJourneys();
  const deleteJourney = useDeleteApiJourneysId();
  const locations = useGetApiLocations();
  const reasons = useGetApiReasons();
  const createTrip = usePostApiJourneysJourneyIdTrips();
  const deleteTrip = useDeleteApiJourneysJourneyIdTripsTripId();

  const form = useForm({
    initialValues: {
      locationId: "",
      reasonId: "",
    },
    validate: {
      locationId: (value) => (value ? null : "Please select a location"),
      reasonId: (value) => (value ? null : "Please select a reason"),
    },
  });

  const navigate = useNavigate();

  // When button is pressed, go back 7 days
  const goToPreviousWeek = () => {
    const prev = new Date(weekStart);
    prev.setDate(prev.getDate() - 7);
    setWeekStart(prev);

    navigate({ to: "/Journeys", search: { weekStart: formatDate(prev) } });
  };

  // When this button is pressed, go forward 7 days
  const goToNextWeek = () => {
    const next = new Date(weekStart);
    next.setDate(next.getDate() + 7);
    setWeekStart(next);

    navigate({ to: "/Journeys", search: { weekStart: formatDate(next) } });
  };

  // Get the name of the day including, day month and year
  const getDayName = (offset: number) => {
    const date = new Date(weekStart);
    date.setDate(date.getDate() + offset);
    return date.toLocaleDateString("en-US", {
      weekday: "short",
      month: "short",
      day: "numeric",
    });
  };

  // Match the journey from the server to the day
  const getJourneyForDay = (offset: number) => {
    if (!journeys.data?.journeys) return null;
    const date = new Date(weekStart);
    date.setDate(date.getDate() + offset);
    const dateStr = formatDate(date);
    return journeys.data.journeys.find((journey) => journey.date === dateStr);
  };

  const handleAddJourney = (offset: number) => {
    const date = new Date(weekStart);
    date.setDate(date.getDate() + offset);
    const dateStr = formatDate(date);

    createJourney.mutate(
      {
        data: {
          date: dateStr,
          isManualMiles: false,
          homeLocationId: 11, // TODO DO NOT KEEP THIS BAD
        },
      },
      {
        onSuccess: async () => {
          await journeys.refetch();
        },
      },
    );
  };

  const handleAddTrip = (journeyId: number) => {
    const validation = form.validate();
    if (!validation.hasErrors) {
      createTrip.mutate(
        {
          journeyId: journeyId,
          data: {
            locationId: form.values.locationId,
            reasonId: form.values.reasonId,
          },
        },
        {
          onSuccess: async () => {
            await journeys.refetch();
            form.reset();
            setShowFormForDay(null);
          },
        },
      );
    }
  };

  const handleDeleteJourney = (journeyId: number) => {
    deleteJourney.mutate(
      {
        id: journeyId,
      },
      {
        onSuccess: async () => {
          await journeys.refetch();
        },
      },
    );
  };

  const handleDeleteTrip = (journeyId: number, tripId: number) => {
    deleteTrip.mutate(
      {
        journeyId: journeyId,
        tripId: tripId,
      },
      {
        onSuccess: async () => {
          await journeys.refetch();
        },
      },
    );
  };

  const populateLocations = () => {
    if (!locations?.data) return [];

    return locations.data.map((location) => ({
      value: location.id?.toString() ?? "", // Select expects string values
      label: location.name ?? "",
    }));
  };

  const populateReasons = () => {
    if (!reasons?.data) return [];

    return reasons.data.map((reason) => ({
      value: reason.id?.toString() ?? "", // Select expects string values
      label: reason.name ?? "",
    }));
  };

  const toggleForm = (offset: number) => {
    if (showFormForDay === offset) {
      setShowFormForDay(null);
      form.reset();
    } else {
      setShowFormForDay(offset);
    }
  };

  return (
    <>
      <Stack gap={"lg"}>
        <Group justify="space-between">
          <Button onClick={goToPreviousWeek} disabled={journeys.isLoading}>
            Previous
          </Button>

          <Text fw={600} size="lg">
            Week of {weekStart.toLocaleDateString()}
          </Text>

          <Button onClick={goToNextWeek} disabled={journeys.isLoading}>
            Next
          </Button>
        </Group>

        {journeys.isLoading && (
          <Center py="xl">
            <Loader />
          </Center>
        )}

        {journeys.isError && (
          <Alert color="red" title="Error">
            Failed to load journeys
          </Alert>
        )}

        {!journeys.isLoading && !journeys.isError && (
          <Stack gap="sm">
            {[0, 1, 2, 3, 4, 5, 6].map((offset) => {
              const journey = getJourneyForDay(offset);

              return (
                <Paper key={offset} withBorder radius="md" p="md">
                  <Text fw={500}>{getDayName(offset)}</Text>
                  {journey ? (
                    <Stack>
                      <>Total Distance: {journey.totalMiles} miles</>
                      <Timeline bulletSize={24} color={"blue"}>
                        <Timeline.Item title={"Home"}>
                          <Text c={"dimmed"} size={"sm"}>
                            Largs
                          </Text>
                        </Timeline.Item>
                        {journey.trips &&
                          journey.trips.map((t) => (
                            <Timeline.Item title={t.locationName} key={t.id}>
                              <Text c={"dimmed"} size={"sm"}>
                                {t.reasonName}{" "}
                                <Anchor
                                  c={"red"}
                                  onClick={() =>
                                    handleDeleteTrip(
                                      Number(journey.id),
                                      Number(t.id),
                                    )
                                  }
                                >
                                  Delete
                                </Anchor>
                              </Text>
                            </Timeline.Item>
                          ))}
                        {showFormForDay === offset && (
                          <Timeline.Item title={"Enter new trip"}>
                            <Group mt="sm">
                              <Select
                                searchable
                                placeholder={"Location"}
                                data={populateLocations()}
                                limit={5}
                                key={form.key("locationId")}
                                {...form.getInputProps("locationId")}
                              />
                              <Select
                                searchable
                                placeholder={"Reason"}
                                data={populateReasons()}
                                limit={5}
                                key={form.key("reasonId")}
                                {...form.getInputProps("reasonId")}
                              />
                              <Button
                                loading={createTrip.isPending}
                                onClick={() =>
                                  handleAddTrip(Number(journey.id))
                                }
                              >
                                Add
                              </Button>
                            </Group>
                          </Timeline.Item>
                        )}
                        {/* Dynamic trips */}
                        <Timeline.Item title={"Home"}>
                          <Text c={"dimmed"} size={"sm"}>
                            Largs
                          </Text>
                        </Timeline.Item>
                      </Timeline>
                      <Button
                        size="xs"
                        variant="light"
                        onClick={() => toggleForm(offset)}
                      >
                        {showFormForDay === offset ? "Exit" : "Edit Trips"}
                      </Button>
                      <Group grow>
                        <CustomButtonLink
                          to={"/Journeys/$journeyId/edit"}
                          size={"xs"}
                          params={{ journeyId: journey.id!.toString() }}
                          search={{ weekStart: formatDate(weekStart) }}
                        >
                          Custom link
                        </CustomButtonLink>
                        <Button
                          size={"xs"}
                          loading={deleteJourney.isPending}
                          color={"red"}
                          onClick={() =>
                            handleDeleteJourney(Number(journey.id))
                          }
                        >
                          Delete
                        </Button>
                      </Group>
                    </Stack>
                  ) : (
                    <Button
                      size={"xs"}
                      onClick={() => handleAddJourney(offset)}
                      loading={createJourney.isPending}
                    >
                      Add Journey
                    </Button>
                  )}
                </Paper>
              );
            })}
          </Stack>
        )}
      </Stack>
    </>
  );
}
