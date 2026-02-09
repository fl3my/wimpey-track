import { useState } from "react";
import {
  useGetApiEmailRecipients,
  usePostApiReportReportIdDraft,
} from "@/api/api-client.gen.ts";
import { Button, Loader, Modal, MultiSelect, Stack, Text } from "@mantine/core";

export function CreateDraftReportEmailModal({
  opened,
  onClose,
  reportId,
}: {
  opened: boolean;
  onClose: () => void;
  reportId: string;
}) {
  const [selectedRecipients, setSelectedRecipients] = useState<string[]>([]);

  const { data: recipients, isLoading, isError } = useGetApiEmailRecipients();

  const createDraft = usePostApiReportReportIdDraft({
    mutation: {
      onSuccess: (result) => {
        if (result.draftId) {
          window.location.href = `https://mail.google.com/mail/u/0/#drafts?compose=${result.draftId}`;
        }
        onClose();
      },
    },
  });

  const handleCreate = () => {
    createDraft.mutate({
      reportId,
      data: { recipientIds: selectedRecipients },
    });
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title="Create Gmail Draft"
      zIndex={3000}
    >
      <Stack>
        {isLoading && <Loader />}
        {isError && <Text c={"red"}>Failed to load recipients</Text>}

        {recipients && (
          <MultiSelect
            comboboxProps={{ zIndex: 3001 }}
            label="Recipients"
            placeholder="Select recipients"
            size={"lg"}
            data={recipients.map((r) => ({
              value: r.id,
              label: `${r.firstName} <${r.email}>`,
            }))}
            value={selectedRecipients}
            onChange={setSelectedRecipients}
            searchable
            required
          />
        )}

        <Button
          onClick={handleCreate}
          disabled={!selectedRecipients.length}
          loading={createDraft.isPending}
        >
          Create Draft
        </Button>
      </Stack>
    </Modal>
  );
}
