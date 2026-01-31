import { defineConfig } from "orval";

export default defineConfig({
  api: {
    input: "http://localhost:5002/openapi/v1.json",
    output: {
      target: "./src/api/api-client.gen.ts",
      client: "react-query",
      httpClient: "fetch",
      override: {
        fetch: {
          includeHttpResponseReturnType: false,
        },
        mutator: {
          path: "./src/api/fetcher.ts",
          name: "fetcher",
        },
      },
      baseUrl: "/api",
    },
  },
  apiZod: {
    input: "http://localhost:5002/openapi/v1.json",
    output: {
      mode: "single",
      client: "zod",
      target: "./src/api/zod.gen.ts",
      override: {
        zod: {
          coerce: {
            param: true,
            query: true,
            header: true,
            body: true,
            response: true,
          },
        },
      },
    },
  },
});
