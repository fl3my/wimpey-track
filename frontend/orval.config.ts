import { defineConfig } from "orval";

export default defineConfig({
  api: {
    input: "http://localhost:5002/openapi/v1.json",
    output: {
      target: "./src/api-client.gen.ts",
      client: "react-query",
      httpClient: "fetch",
      override: {
        fetch: {
          includeHttpResponseReturnType: false,
        },
      },
      baseUrl: "/api",
    },
  },
});
