import { createFileRoute, useNavigate } from "@tanstack/react-router";
import {
  ActionIcon,
  Alert,
  Button,
  Center,
  Group,
  Loader,
  Paper,
  Select,
  Stack,
  Text,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import {
  useDeleteApiJourneysId,
  useGetApiJourneys,
  useGetApiLocations,
  useGetApiReasons,
  usePostApiJourneys,
} from "@/api/api-client.gen.ts";
import { useState } from "react";
import z from "zod";
import { zod4Resolver } from "mantine-form-zod-resolver";
import { LocationSortBy } from "@/routes/Locations";
import { IconArrowLeft, IconArrowRight, IconTrash } from "@tabler/icons-react";

const tripSchema = z.object({
  locationId: z.coerce.number().int().positive(),
  reasonId: z.coerce.number().int().positive(),
});

export const journeySchema = z.object({
  trips: z.array(tripSchema).min(1),
});

export const Route = createFileRoute("/Journeys/")({
  component: RouteComponent,
  validateSearch: (search: { weekStart?: string }) => {
    return {
      weekStart: search.weekStart,
    };
  },
});

function RouteComponent() {
  const { weekStart } = Route.useSearch();
  const navigate = useNavigate();

  const [addingForDate, setAddingForDate] = useState<string | null>(null);

  const form = useForm({
    initialValues: {
      trips: [
        {
          locationId: "",
          reasonId: "",
        },
      ],
    },
    validate: zod4Resolver(journeySchema),
  });

  const journeysByWeek = useGetApiJourneys({ weekStart });
  const locationsQuery = useGetApiLocations({
    sortBy: LocationSortBy.TripCount,
  });
  const reasonsQuery = useGetApiReasons();
  const createJourney = usePostApiJourneys();
  const deleteJourney = useDeleteApiJourneysId();

  const locationOptions =
    locationsQuery.data?.map((l) => ({
      value: String(l.id)!,
      label: l.name ?? "Unknown",
    })) ?? [];

  const reasonOptions =
    reasonsQuery.data?.map((r) => ({
      value: String(r.id)!,
      label: r.name ?? "Unknown",
    })) ?? [];

  if (journeysByWeek.isLoading) {
    return (
      <Center py="xl">
        <Loader />
      </Center>
    );
  }

  if (journeysByWeek.isError || !journeysByWeek.data) {
    return <Alert color="red">Failed to load journeys</Alert>;
  }

  const handleSubmit = (values: typeof form.values) => {
    if (!addingForDate) return;

    // Convert select string values to numbers for id
    const parsed = journeySchema.parse(values);

    createJourney.mutate(
      {
        data: {
          date: addingForDate,
          trips: parsed.trips,
        },
      },
      {
        onSuccess: async () => {
          await journeysByWeek.refetch();
          await locationsQuery.refetch();
          setAddingForDate(null);
          form.reset();
        },
      },
    );
  };

  const handleDelete = (journeyId: number) => {
    deleteJourney.mutate(
      { id: journeyId },
      {
        onSuccess: async () => {
          await journeysByWeek.refetch();
          await locationsQuery.refetch();
        },
      },
    );
  };

  const {
    days,
    weekStart: resolvedWeekStart,
    prevWeekStart,
    nextWeekStart,
  } = journeysByWeek.data;

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Button
          leftSection={<IconArrowLeft size={16} />}
          onClick={() =>
            navigate({
              to: "/Journeys",
              search: { weekStart: prevWeekStart },
            })
          }
        >
          Previous
        </Button>

        <Text fw={600}>
          Week of {new Date(resolvedWeekStart).toLocaleDateString("en-US")}
        </Text>

        <Button
          rightSection={<IconArrowRight size={16} />}
          onClick={() =>
            navigate({
              to: "/Journeys",
              search: { weekStart: nextWeekStart },
            })
          }
        >
          Next
        </Button>
      </Group>

      <Stack>
        {days.map((day) => (
          <Paper key={day.date} withBorder p="md">
            <Text fw={500}>
              {new Date(day.date).toLocaleDateString("en-gb", {
                weekday: "long",
                month: "long",
                day: "numeric",
              })}
            </Text>
            {day.journey && (
              <Text size="sm" c="dimmed">
                Total miles: {day.journey.totalMiles}
              </Text>
            )}
            {day.journey ? (
              <Stack mt="sm">
                <Group>
                  <Text>{day.journey.homeLocationName}</Text>
                  <Text c="dimmed">{"Home (start)"}</Text>
                </Group>
                {day.journey.trips?.map((trip) => (
                  <Group key={trip.id}>
                    <Text>{trip.locationName}</Text>
                    <Text c="dimmed">{trip.reasonName}</Text>
                  </Group>
                ))}
                <Group>
                  <Text>{day.journey.homeLocationName}</Text>
                  <Text c="dimmed">{"Home (end)"}</Text>
                </Group>
                <Button
                  color="red"
                  variant="light"
                  onClick={() => handleDelete(Number(day?.journey?.id))}
                >
                  Delete journey
                </Button>
              </Stack>
            ) : addingForDate === day.date ? (
              <Paper mt="sm" p="sm" withBorder>
                <form onSubmit={form.onSubmit(handleSubmit)}>
                  <Stack>
                    {form.values.trips.map((_, index) => (
                      <Group key={index}>
                        <Group grow flex={1}>
                          <Select
                            searchable
                            placeholder={"Location"}
                            data={locationOptions}
                            limit={10}
                            {...form.getInputProps(`trips.${index}.locationId`)}
                          />
                          <Select
                            searchable
                            placeholder={"Reason"}
                            data={reasonOptions}
                            limit={10}
                            {...form.getInputProps(`trips.${index}.reasonId`)}
                          />
                        </Group>
                        <ActionIcon
                          color="red"
                          size={"lg"}
                          disabled={form.values.trips.length === 1}
                          onClick={() => form.removeListItem("trips", index)}
                        >
                          <IconTrash size={16} />
                        </ActionIcon>
                      </Group>
                    ))}

                    <Button
                      variant="light"
                      onClick={() =>
                        form.insertListItem("trips", {
                          locationId: "",
                          reasonId: "",
                        })
                      }
                    >
                      Add another trip
                    </Button>

                    <Group justify="flex-end">
                      <Button
                        variant="subtle"
                        onClick={() => setAddingForDate(null)}
                      >
                        Cancel
                      </Button>

                      <Button type="submit">Save journey</Button>
                    </Group>
                  </Stack>
                </form>
              </Paper>
            ) : (
              <Button
                mt="sm"
                onClick={() => {
                  form.reset();
                  setAddingForDate(day.date);
                }}
              >
                Add journey
              </Button>
            )}
          </Paper>
        ))}
      </Stack>
    </Stack>
  );
}
