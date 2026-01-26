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
  usePostReceiptsOcr,
  usePostReceiptsUploadBase64,
  usePostPurchases,
} from "@/api-client.gen.ts";
import {
  PurchaseForm,
  type PurchaseFormValues,
} from "@/components/purchase-form.tsx";
import { FuelForm, type FuelFormValues } from "@/components/fuel-form.tsx";

export const Route = createFileRoute("/Receipts/ocr")({
  component: RouteComponent,
});

function RouteComponent() {
  const { mutate, isPending, data } = usePostReceiptsOcr();
  const { mutateAsync: createReceipt } = usePostReceiptsUploadBase64();
  const { mutateAsync: createPurchase } = usePostPurchases();

  const navigate = useNavigate();

  const uploadField = useField({
    initialValue: undefined as File | undefined,
  });

  const onHandleUpload = () => {
    const file = uploadField.getValue();
    if (!file) return;
    mutate({ data: { File: file } });
  };

  const onHandleSubmitPurchase = async (values: PurchaseFormValues) => {
    console.log(values);
    if (!data?.imageBase64) return;

    const date = values.date!.toISOString().split("T")[0];

    try {
      const receipt = await createReceipt({
        data: {
          name: values.storeName,
          date: date,
          category: 0,
          base64Content: data.imageBase64,
        },
      });

      const purchase = await createPurchase({
        data: {
          date: date,
          storeName: values.storeName,
          items: values.items,
          receiptId: receipt.id,
        },
      });

      console.log(purchase);

      await navigate({
        to: "/Purchases/$purchaseId",
        params: { purchaseId: purchase?.id!.toString() },
      });
    } catch (error) {
      console.log(error);
    }
  };

  const onHandleSubmitFuel = async (values: FuelFormValues) => {
    console.log(values);

    const date = values.date!.toISOString().split("T")[0];

    try {
      const receipt = await createReceipt({
        data: {
          name: values.name,
          date: date,
          category: 1,
          base64Content: data?.imageBase64!,
        },
      });

      await navigate({
        to: "/Receipts/$receiptId",
        params: { receiptId: receipt.id!.toString() },
      });
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <Stack gap={"lg"}>
      <Paper p={"md"}>
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
            disabled={isPending}
            loading={isPending}
          >
            {isPending ? "Processing..." : "Process Receipt"}
          </Button>
        </Group>
        {isPending && (
          <Stack align="center" gap="xs">
            <Loader size="sm" />
            <Text size="sm" c="dimmed">
              Analyzing receipt...
            </Text>
          </Stack>
        )}
      </Paper>

      {data?.receiptData && data.receiptData.length === 0 && (
        <Alert color="blue" mt="md">
          No receipt data found. Please try uploading a clearer image.
        </Alert>
      )}

      {data?.receiptData?.map((receipt, index) => {
        const imageSrc = `data:image/jpeg;base64,${data.imageBase64}`;

        switch (receipt.receiptCategory) {
          case 0:
            return (
              <div key={index}>
                <Image h={300} fit={"contain"} src={imageSrc} alt="Receipt" />
                <PurchaseForm
                  initialValues={{
                    date:
                      new Date(receipt?.transactionDate!.toString()) ?? null,
                    storeName: receipt.storeName,
                    items: receipt?.receiptItems!.map((item) => ({
                      cost: Number(item.price),
                      name: item.description ?? "",
                      quantity: Number(item.quantity),
                      reason: "",
                    })),
                  }}
                  onSubmit={onHandleSubmitPurchase}
                />
              </div>
            );

          case 1:
            return (
              <div key={index}>
                <Image h={300} fit={"contain"} src={imageSrc} alt="Receipt" />
                <FuelForm
                  initialValues={{
                    date:
                      new Date(receipt?.transactionDate!.toString()) ?? null,
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
