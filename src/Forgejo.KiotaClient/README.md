## Kiota Client Generation Strategy

This project uses a **two‑layer structure** for the Forgejo API client:

- **`Generated/`** — contains all Kiota‑generated code.  
  This directory is fully disposable and can be safely deleted and regenerated at any time.

- **`Overrides/`** — contains custom logic, ergonomic helpers, stability wrappers, and patches for
  incomplete or incorrect OpenAPI definitions.  
  These files are *never* overwritten by Kiota and represent the durable part of the SDK.

This split ensures long‑term maintainability: the generated client can evolve with Forgejo’s
OpenAPI, while our overrides remain stable and under our control.

### Generate the client

```bash
kiota generate \
  --openapi https://forgejo.home/swagger.v1.json \
  --language csharp \
  --output ./Generated \
  --namespace-name MarkZither.Forgejo.ApiClient \
  --class-name ForgejoClient
```

### Generate the Kiota client (PowerShell)

```powershell
kiota generate `
  --openapi https://forgejo.home/swagger.v1.json `
  --language csharp `
  --output ./Generated `
  --namespace-name MarkZither.Forgejo.ApiClient `
  --class-name ForgejoClient
```

If you want an even more PowerShell‑native style (no backticks, just a single long line), here’s the alternative:

### Generate the Kiota client (PowerShell, single line)

```powershell
kiota generate --openapi https://forgejo.home/swagger.v1.json --language csharp --output ./Generated  --namespace-name MarkZither.Forgejo.ApiClient --class-name ForgejoClient
```
