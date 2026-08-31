# Pull Request Workflow Client and Auth

**Status**: Accepted
**Date**: 2026-08-30

## Context

The Forgejo MCP server needs a reliable, typed client for repository pull request operations. The project must work with self-hosted Forgejo instances that share GitHub-compatible API semantics while preserving strong validation for repository policy, permissions, and branch state.

The implementation also needs an authentication model that is secure enough for local agent workflows and scoped enough to avoid over-broad repository control. The client must allow PR creation, comment reads and writes, and merge attempts without being tightly coupled to a specific UI or editor flow.

## Priorities and Requirements (ordered)

1. **Stable repository contract** — The API layer must reflect the Forgejo REST contract consistently across PR operations and handle validation failures without pretending a request succeeded.
2. **Least-privilege access** — Auth credentials must be scoped to the target repository and should not assume admin or cross-repo permissions.
3. **Maintainability** — The client should be generated or modeled in a typed way that keeps MCP tool logic aligned with API changes and remains easy to extend for other Forgejo workflow features.

## Options Considered

### Option 1: Handwritten HttpClient wrapper

A custom client would directly call Forgejo REST endpoints and model request/response objects in the MCP server project.

**Evaluation against priorities**:
- **Stable repository contract**: Partially meets. It can be explicit, but it is easier to drift from actual Forgejo API changes and harder to validate across all PR operations.
- **Least-privilege access**: Meets. The request layer can limit tokens and headers to repository-scoped configuration.
- **Maintainability**: Partially meets. It is simple initially, but repeated endpoint and model logic increases long-term drift and testing burden.

### Option 2: Kiota-backed typed client with repository-scoped auth

A Kiota-generated client models the Forgejo endpoints and serializes typed requests, while the runtime configuration supplies a repository-scoped token or bearer credential.

**Evaluation against priorities**:
- **Stable repository contract**: Meets. Kiota keeps the API model structured, easier to validate, and aligned with endpoint contracts.
- **Least-privilege access**: Meets. Token handling remains centralized in configuration and request pipeline, which makes repository scope enforcement explicit.
- **Maintainability**: Meets. The typed client is easier to extend as PR and issue workflow features expand.

## Decision

The project will use a Kiota-backed typed Forgejo client and a repository-scoped configuration-based auth model. This approach best satisfies the need for a stable API contract, explicit permission boundaries, and low-maintenance extension as the feature set grows.

The auth layer will accept a Forgejo token or equivalent bearer credential from the runtime configuration and attach it only to the repository API requests required for the current operation. The server will not bypass Forgejo branch protection or merge policy checks.

## Implementation Notes

- The first implementation should focus on PR creation, read, comment, and merge flows using the GitHub-compatible Forgejo REST API.
- The HTTP pipeline should centralize auth headers and structured error mapping so all PR operations can surface actionable repository policy failures.
- The client should remain versioned with the transport contract so future Forgejo API differences are surfaced as explicit compatibility checks.

## References

* [docs/features/pull-request-workflow/spec.md](../features/pull-request-workflow/spec.md)
* [docs/envisioning/README.md](../envisioning/README.md)
* [README.md](../../README.md)
