import { createFileRoute } from "@tanstack/react-router";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import {
  useGetApiLocations,
  useGetApiProfile,
  usePutApiProfile,
} from "@/api/api-client.gen.ts";
import { useForm } from "@mantine/form";
import { putApiProfileBody } from "@/api/zod.gen.ts";
import { zod4Resolver } from "mantine-form-zod-resolver";
import z from "zod";
import { useEffect } from "react";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import {
  Alert,
  Button,
  NumberInput,
  Select,
  TextInput,
  Title,
} from "@mantine/core";
import { useState } from "react";

export const Route = createFileRoute("/Profile/")({
  component: RouteComponent,
});

function RouteComponent() {
  const serverErrors = useServerErrors();
  const [saved, setSaved] = useState(false);

  // Get the location options for the dropdown
  const locationsQuery = useGetApiLocations();
  const locationOptions =
    locationsQuery.data?.map((l) => ({
      value: String(l.id)!,
      label: l.name ?? "Unknown",
    })) ?? [];

  const profile = useGetApiProfile();

  // Prepopulate the form
  useEffect(() => {
    if (!profile.data) return;
    form.setValues(profile.data);
    form.resetDirty(profile.data);
  }, [profile.data]);

  const form = useForm<z.infer<typeof putApiProfileBody>>({
    initialValues: {
      fullName: "",
      staffNumber: "",
      businessUnit: "",
      departmentSiteName: "",
      vehicleFuelType: "",
      vehicleEngineSize: 0,
      vehicleRegistration: "",
      vehicleMake: "",
      homePostcode: "",
      homeLocationId: 0,
    },
    validate: zod4Resolver(putApiProfileBody),
  });

  const mutation = usePutApiProfile({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
        setSaved(false);
      },
      onSuccess: async () => {
        serverErrors.clear();
        setSaved(true);
      },
    },
  });

  const handleSubmit = (values: typeof form.values) => {
    setSaved(false);
    form.clearErrors();
    mutation.mutate({
      data: {
        fullName: values.fullName,
        staffNumber: values.staffNumber,
        businessUnit: values.businessUnit,
        departmentSiteName: values.departmentSiteName,
        vehicleFuelType: values.vehicleFuelType,
        vehicleEngineSize: values.vehicleEngineSize,
        vehicleRegistration: values.vehicleRegistration,
        vehicleMake: values.vehicleMake,
        homePostcode: values.homePostcode,
        homeLocationId: values.homeLocationId,
      },
    });
  };

  return (
    <>
      <Title order={2} mb={"md"}>
        Edit Profile
      </Title>

      <form onSubmit={form.onSubmit(handleSubmit)}>
        <ServerErrorAlert errors={serverErrors.errors} />
        {saved && (
          <Alert color="green" mb="md">
            Profile saved successfully.
          </Alert>
        )}
        <TextInput
          label={"Full Name"}
          placeholder={"Full Name"}
          key={form.key("fullName")}
          {...form.getInputProps("fullName")}
        />
        <TextInput
          label={"Staff Number"}
          placeholder={"Staff Number"}
          key={form.key("staffNumber")}
          {...form.getInputProps("staffNumber")}
        />
        <TextInput
          label={"Business Unit"}
          placeholder={"Business Unit"}
          key={form.key("businessUnit")}
          {...form.getInputProps("businessUnit")}
        />
        <TextInput
          label={"Department Site Name"}
          placeholder={"Department Site Name"}
          key={form.key("departmentSiteName")}
          {...form.getInputProps("departmentSiteName")}
        />
        <TextInput
          label={"Vehicle Fuel Type"}
          placeholder={"Vehicle Fuel Type"}
          key={form.key("vehicleFuelType")}
          {...form.getInputProps("vehicleFuelType")}
        />
        <NumberInput
          label={"Vehicle Engine Size"}
          key={form.key("vehicleEngineSize")}
          {...form.getInputProps("vehicleEngineSize")}
        />
        <TextInput
          label={"Vehicle Registration"}
          placeholder={"Vehicle Registration"}
          key={form.key("vehicleRegistration")}
          {...form.getInputProps("vehicleRegistration")}
        />
        <TextInput
          label={"Vehicle Make"}
          placeholder={"Vehicle Make"}
          key={form.key("vehicleMake")}
          {...form.getInputProps("vehicleMake")}
        />
        <Select
          searchable
          label={"Home Location"}
          placeholder={"Home Location"}
          data={locationOptions}
          key={form.key("homeLocationId")}
          value={
            form.values.homeLocationId
              ? String(form.values.homeLocationId)
              : null
          }
          onChange={(value) =>
            form.setFieldValue("homeLocationId", value ? Number(value) : 0)
          }
        />

        <Button
          type="submit"
          loading={mutation.isPending}
          disabled={mutation.isPending}
        >
          Submit
        </Button>
      </form>
    </>
  );
}
