import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { usePostReceipts } from "@/api/api-client.gen.ts";
import { CustomButtonLink } from "@/components/custom-button-link.tsx";
import {
  ReceiptForm,
  type ReceiptFormValues,
} from "@/components/receipt-form.tsx";

export const Route = createFileRoute("/Receipts/new")({
  component: RouteComponent,
});

function RouteComponent() {
  const navigate = useNavigate();
  const createReceipt = usePostReceipts();
  const handleSubmit = async (values: ReceiptFormValues) => {
    try {
      console.log(values);
      await createReceipt.mutateAsync({
        data: {
          Name: values.name,
          Date: values.date?.toString(),
          Category: values.category,
          File: values.file!,
        },
      });

      await navigate({ to: "/Receipts" });
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <>
      <CustomButtonLink to={"/Receipts"}>Back</CustomButtonLink>
      <ReceiptForm onSubmit={handleSubmit} />
    </>
  );
}
