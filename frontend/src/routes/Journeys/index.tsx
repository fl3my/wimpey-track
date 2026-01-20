import { createFileRoute } from "@tanstack/react-router";
import {
  useDeleteApiJourneysId,
  useGetApiJourneys,
  useGetApiLocations,
  useGetApiReasons,
  usePostApiJourneys,
  usePostApiJourneysJourneyIdTrips,
} from "@/api-client.gen.ts";
import { useEffect, useState } from "react";
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
} from "@mantine/core";

export const Route = createFileRoute("/Journeys/")({
  component: RouteComponent,
});

function RouteComponent() {
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
  const [weekStart, setWeekStart] = useState(getMonday(new Date()));
  const [selectedLocation, setSelectedLocation] = useState<string | null>(null);
  const [selectedReason, setSelectedReason] = useState<string | null>(null);
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

  useEffect(() => {
    if (
      createJourney.isSuccess ||
      deleteJourney.isSuccess ||
      createTrip.isSuccess
    ) {
      journeys.refetch();
    }
  }, [createJourney.isSuccess, deleteJourney.isSuccess, createTrip.isSuccess]);

  // When button is pressed, go back 7 days
  const goToPreviousWeek = () => {
    const prev = new Date(weekStart);
    prev.setDate(prev.getDate() - 7);
    setWeekStart(prev);
  };

  // When this button is pressed, go forward 7 days
  const goToNextWeek = () => {
    const next = new Date(weekStart);
    next.setDate(next.getDate() + 7);
    setWeekStart(next);
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

    createJourney.mutate({
      data: {
        date: dateStr,
        isManualMiles: false,
        homeLocationId: 1, // TODO DO NOT KEEP THIS BAD
      },
    });
  };

  const handleAddTrip = (journeyId: number) => {
    if (selectedLocation && selectedReason) {
      createTrip.mutate({
        journeyId: journeyId,
        data: {
          locationId: selectedLocation,
          reasonId: selectedReason,
        },
      });
    }
  };

  const handleDeleteJourney = (journeyId: number) => {
    deleteJourney.mutate({
      id: journeyId,
    });
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
    setShowFormForDay(showFormForDay === offset ? null : offset);
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
                                {t.reasonName}
                              </Text>
                            </Timeline.Item>
                          ))}
                        {showFormForDay === offset && (
                          <Timeline.Item title={"Enter new trip"}>
                            <Group mt="sm">
                              <Select
                                searchable
                                value={selectedLocation}
                                onChange={setSelectedLocation}
                                placeholder={"Location"}
                                data={populateLocations()}
                              />
                              <Select
                                searchable
                                value={selectedReason}
                                onChange={setSelectedReason}
                                placeholder={"Reason"}
                                data={populateReasons()}
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
                        <Button size={"xs"}>Edit Details</Button>
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
