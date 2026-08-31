# Review: Pull Request Workflow

**Date**: 2026-08-30
**Validated artifacts**: [docs/features/pull-request-workflow/spec.md](spec.md), [docs/architecture/decisions/pr-workflow-client-auth.md](../../architecture/decisions/pr-workflow-client-auth.md), [docs/features/pull-request-workflow/tasks.md](tasks.md)
**Reviewed code**: [src/Forgejo.McpServer/Program.cs](../../src/Forgejo.McpServer/Program.cs), [src/Forgejo.McpServer/Tools/RandomNumberTools.cs](../../src/Forgejo.McpServer/Tools/RandomNumberTools.cs), [src/Forgejo.McpServer/Forgejo.McpServer.csproj](../../src/Forgejo.McpServer/Forgejo.McpServer.csproj)

## Result

**Status**: FAILED
- Critical: 0
- Major: 3
- Minor: 0

## Checklist

### Spec Compliance

| ID | Requirement | Status | Evidence |
|----|-------------|--------|----------|
| FR-001 | Create PR from valid branch change | ❌ FAIL | The server exposes only a random-number tool; no PR creation tool or service exists in [src/Forgejo.McpServer/Program.cs](../../src/Forgejo.McpServer/Program.cs) and [src/Forgejo.McpServer/Tools/RandomNumberTools.cs](../../src/Forgejo.McpServer/Tools/RandomNumberTools.cs). |
| FR-004 | Add PR comments | ❌ FAIL | No comment model, service, or endpoint exists for PR review discussion. |
| FR-005 | Merge eligible PRs | ❌ FAIL | There is no merge endpoint, service, or validation pipeline for PR state checks. |
| FR-006 | Clear failure reporting | ❌ FAIL | The implementation has no Forgejo-specific error mapping or repository policy handling. |
| FR-007 | Never claim success without API confirmation | ❌ FAIL | No Forgejo mutation path exists, so the API confirmation rule is not yet implemented. |

### Conformance Test Mapping

| CC-ID | Scenario | Test Case | Status |
|-------|----------|-----------|--------|
| CC-001 | Happy path PR creation | none found | ❌ Missing |
| CC-002 | PR comment creation | none found | ❌ Missing |
| CC-003 | Merge success | none found | ❌ Missing |
| CC-004 | Policy failure | none found | ❌ Missing |
| CC-005 | Must NOT happen | none found | ❌ Missing |

### ADR Compliance

| ADR | Constraint | Status | Evidence |
|-----|-----------|--------|----------|
| Pull Request Workflow Client and Auth | Use a Kiota-backed typed Forgejo client and repository-scoped auth | ❌ FAIL | The project still contains the default MCP server template and random-number tool instead of a Kiota client or Forgejo auth pipeline in [src/Forgejo.McpServer/Program.cs](../../src/Forgejo.McpServer/Program.cs) and [src/Forgejo.McpServer/Forgejo.McpServer.csproj](../../src/Forgejo.McpServer/Forgejo.McpServer.csproj). |

### Build & Tests

| Command | Result |
|---------|--------|
| dotnet build --nologo | ✅ PASS |
| dotnet test --nologo --verbosity minimal | ✅ PASS |

> The build and test commands pass only because the repository currently contains the default template app with no feature-level implementation or regression coverage. The pass result is not evidence that the PR workflow requirement is satisfied.

## Findings

### Major

- **M-001**: Pull request workflow is not implemented
  - Expected: The feature spec requires PR creation, review comments, and merge capabilities with repository policy handling in [docs/features/pull-request-workflow/spec.md](spec.md).
  - Found: The app is still the template MCP server with a random-number tool; there are no PR tools, no Forgejo client, and no state checks.
  - File: [src/Forgejo.McpServer/Program.cs](../../src/Forgejo.McpServer/Program.cs), [src/Forgejo.McpServer/Tools/RandomNumberTools.cs](../../src/Forgejo.McpServer/Tools/RandomNumberTools.cs)
  - Suggested fix: Implement the PR models, service layer, and MCP tool surface required by the workflow before claiming feature readiness.

- **M-002**: ADR decision is not reflected in implementation
  - Expected: The ADR requires a Kiota-backed typed client and scoped auth model in [docs/architecture/decisions/pr-workflow-client-auth.md](../../architecture/decisions/pr-workflow-client-auth.md).
  - Found: The project is still configured as a generic MCP example app with no Kiota package references or auth pipeline.
  - File: [src/Forgejo.McpServer/Forgejo.McpServer.csproj](../../src/Forgejo.McpServer/Forgejo.McpServer.csproj), [src/Forgejo.McpServer/Program.cs](../../src/Forgejo.McpServer/Program.cs)
  - Suggested fix: Add the Kiota client package and configure the HTTP/auth pipeline consistent with the ADR before implementing PR operations.

- **M-003**: No conformance or regression tests cover the PR workflow
  - Expected: The spec requires conformance cases for PR creation, comment creation, merge success, policy rejection, and non-existent PR failure in [docs/features/pull-request-workflow/spec.md](spec.md).
  - Found: The tests folder is empty, and no workflow-level tests exist.
  - File: [tests](../../tests)
  - Suggested fix: Add failing regression tests for the PR workflow before implementing the feature logic.

## Next Steps

The current implementation does not satisfy the pull request workflow spec or the approved ADR. The project must complete the foundational work for Kiota client wiring, domain models, auth handling, and PR endpoints before a review can pass.
