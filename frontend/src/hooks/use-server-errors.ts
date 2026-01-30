import { useState } from "react";

export function useServerErrors() {
  const [errors, setErrors] = useState<string[]>([]);

  const setFromApiError = (error: unknown) => {
    // If ValidationProblemDetails return type
    const apiErrors = (error as any)?.errors as
      | Record<string, string[]>
      | undefined;

    if (apiErrors) {
      setErrors(Object.values(apiErrors).flat());
      return;
    }

    // General api message
    const apiMessage = (error as any)?.message;

    setErrors([apiMessage ?? "Something went wrong."]);
  };

  const clear = () => setErrors([]);

  return {
    errors,
    setFromApiError,
    clear,
    hasError: errors.length > 0,
  };
}
