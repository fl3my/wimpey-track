import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useField } from "@mantine/form";
import {
  Alert,
  Button,
  FileInput,
  Group,
  Image,
  Loader,
  Paper,
  Stack,
  Text,
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
      <Paper p={"md"}>
        <ServerErrorAlert errors={serverErrors.errors} />
        <Group align={"end"}>
          <FileInput
            label={"Receipt Image Upload"}
            accept={"image/jpeg"}
            capture={"environment"}
            placeholder={"Receipt Image Upload"}
            clearable
            {...uploadField.getInputProps()}
          />
          <Button
            onClick={onHandleUpload}
            disabled={!uploadField.getValue() || ocr.isPending}
            loading={ocr.isPending}
          >
            {ocr.isPending ? "Processing..." : "Process Receipt"}
          </Button>
        </Group>
        {ocr.isPending && (
          <Stack align="center" gap="xs">
            <Loader size="sm" />
            <Text size="sm" c="dimmed">
              Analyzing receipt...
            </Text>
          </Stack>
        )}
      </Paper>

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
                <Image h={300} fit={"contain"} src={imageSrc} alt="Receipt" />
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
                <Image h={300} fit={"contain"} src={imageSrc} alt="Receipt" />
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
