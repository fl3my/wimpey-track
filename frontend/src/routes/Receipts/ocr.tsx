import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useField } from "@mantine/form";
import {
  Alert,
  Button,
  Card,
  Center,
  FileInput,
  Group,
  Image,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import {
  type CreateFuelReceiptDto,
  type CreatePurchaseReceiptDto,
  usePostApiReceiptsFuel,
  usePostApiReceiptsOcr,
  usePostApiReceiptsPurchase,
} from "@/api/api-client.gen.ts";
import {
  PurchaseForm,
  type PurchaseFormValues,
} from "@/components/purchase-form.tsx";
import { FuelForm, type FuelFormValues } from "@/components/fuel-form.tsx";
import { useServerErrors } from "@/hooks/use-server-errors.ts";
import { ServerErrorAlert } from "@/components/server-error-alert.tsx";
import { CustomLink } from "@/components/custom-link.tsx";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";

enum ReceiptCategory {
  Purchase = 0,
  Fuel = 1,
}

export const Route = createFileRoute("/Receipts/ocr")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();
  const serverErrors = useServerErrors();

  const ocr = usePostApiReceiptsOcr({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: () => {
        serverErrors.clear();
      },
    },
  });

  const uploadFuelReceipt = usePostApiReceiptsFuel({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: async (values) => {
        serverErrors.clear();
        await navigate({
          to: "/Receipts/$receiptId",
          params: { receiptId: String(values.id) },
        });
      },
    },
  });

  const uploadPurchaseReceipt = usePostApiReceiptsPurchase({
    mutation: {
      onError: (error) => {
        serverErrors.setFromApiError(error);
      },
      onSuccess: async (values) => {
        serverErrors.clear();
        await navigate({
          to: "/Receipts/$receiptId",
          params: { receiptId: String(values.id) },
        });
      },
    },
  });

  const uploadField = useField({
    initialValue: undefined as File | undefined,
  });

  const onHandleUpload = () => {
    const file = uploadField.getValue();
    if (!file) return;

    ocr.mutate({ data: { File: file } });
  };

  const onHandleSubmitPurchase = (values: PurchaseFormValues) => {
    if (!ocr.data?.imageBase64) {
      serverErrors.setFromApiError({
        message: "Please process a receipt image first.",
      });
      return;
    }

    const receipt: CreatePurchaseReceiptDto = {
      base64Content: ocr.data.imageBase64,
      date: values.date,
      name: values.storeName,
      purchase: { ...values },
    };

    uploadPurchaseReceipt.mutate({ data: receipt });
  };

  const onHandleSubmitFuel = (values: FuelFormValues) => {
    if (!ocr.data?.imageBase64) {
      serverErrors.setFromApiError({
        message: "Please process a receipt image first.",
      });
      return;
    }

    const receipt: CreateFuelReceiptDto = {
      name: values.name,
      date: values.date,
      base64Content: ocr.data?.imageBase64 ?? "",
    };
    uploadFuelReceipt.mutate({ data: receipt });
  };

  return (
    <Stack gap={"lg"}>
      <ServerErrorAlert errors={serverErrors.errors} />
      <Card withBorder radius={"md"}>
        <Stack>
          <Title order={2}>Automatic Receipt</Title>
          <Text size="sm" c="dimmed">
            Create a receipt automatically. This will automatically detect the
            type of receipt, as well as extract information from it.
          </Text>
          <FileInput
            label={"Receipt Image Upload"}
            accept={"image/jpeg"}
            capture={"environment"}
            placeholder={"Receipt Image Upload"}
            clearable
            {...uploadField.getInputProps()}
          />
          <Group justify={"flex-end"}>
            <CustomButtonLink to={"/Receipts"} variant={"light"}>
              Cancel
            </CustomButtonLink>
            <Button
              onClick={onHandleUpload}
              disabled={!uploadField.getValue() || ocr.isPending}
              loading={ocr.isPending}
            >
              {ocr.isPending ? "Processing..." : "Process Receipt"}
            </Button>
          </Group>
          {serverErrors.errors.length > 0 && (
            <CustomLink to={"/Receipts/new"} c={"red"}>
              Not working? Try manual instead
            </CustomLink>
          )}
        </Stack>
      </Card>

      {ocr.data?.receiptData && ocr.data.receiptData.length === 0 && (
        <Alert color="blue" mt="md">
          No receipt data found. Please try uploading a clearer image.
        </Alert>
      )}

      {ocr.data?.receiptData?.map((receipt, index) => {
        const imageSrc = `data:image/jpeg;base64,${ocr.data.imageBase64}`;

        switch (receipt.receiptCategory) {
          case ReceiptCategory.Purchase:
            return (
              <div key={index}>
                <Center my={"md"}>
                  <Image
                    maw={200}
                    fit={"contain"}
                    src={imageSrc}
                    alt="Receipt"
                  />
                </Center>

                <PurchaseForm
                  initialValues={{
                    date: receipt.transactionDate ?? "",
                    storeName: receipt.storeName ?? "",
                    items:
                      receipt.receiptItems?.map((item) => ({
                        cost: Number(item.price),
                        name: item.description ?? "",
                        quantity: Number(item.quantity),
                        reason: "",
                      })) ?? [],
                  }}
                  onSubmit={onHandleSubmitPurchase}
                />
              </div>
            );

          case ReceiptCategory.Fuel:
            return (
              <div key={index}>
                <Center my={"md"}>
                  <Image
                    maw={200}
                    fit={"contain"}
                    src={imageSrc}
                    alt="Receipt"
                  />
                </Center>
                <FuelForm
                  initialValues={{
                    date: receipt?.transactionDate?.toString() ?? "",
                    name: receipt.storeName ?? "",
                  }}
                  onSubmit={onHandleSubmitFuel}
                />
              </div>
            );

          default:
            return null;
        }
      })}
    </Stack>
  );
}
