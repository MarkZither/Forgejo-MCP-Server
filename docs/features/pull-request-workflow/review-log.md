# Review: Pull Request Workflow

**Date**: 2026-09-10
**Validated artifacts**: [docs/features/pull-request-workflow/spec.md](spec.md), [docs/architecture/decisions/pr-workflow-client-auth.md](../../architecture/decisions/pr-workflow-client-auth.md), [docs/features/pull-request-workflow/tasks.md](tasks.md)
**Reviewed code**: [src/Forgejo.McpServer/Program.cs](../../src/Forgejo.McpServer/Program.cs), [src/Forgejo.McpServer/Services/ForgejoClientFactory.cs](../../src/Forgejo.McpServer/Services/ForgejoClientFactory.cs), [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs), [src/Forgejo.McpServer/Models/PullRequestCreateRequest.cs](../../src/Forgejo.McpServer/Models/PullRequestCreateRequest.cs), [src/Forgejo.McpServer/Tools/ForgejoPullRequestTools.cs](../../src/Forgejo.McpServer/Tools/ForgejoPullRequestTools.cs), [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs)

## Result

**Status**: PASSED_WITH_FINDINGS
- Critical: 0
- Major: 2
- Minor: 0

## Checklist

### Spec Compliance

| ID | Requirement | Status | Evidence |
|----|-------------|--------|----------|
| FR-001 | Create PR from valid branch change | ✅ PASS | The tool and service flow exist in [src/Forgejo.McpServer/Tools/ForgejoPullRequestTools.cs](../../src/Forgejo.McpServer/Tools/ForgejoPullRequestTools.cs) and [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |
| FR-006 | Clear failure reporting | ✅ PASS | Repository failures are mapped to structured results in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |
| FR-004 | Review and comment workflow | ❌ FAIL | No comment or review-thread implementation exists in the current codebase or task status. |
| FR-005 | Merge eligible PRs | ❌ FAIL | No merge models, service, or endpoint flow is present yet. |
| CC-001 | Happy path PR creation | ⚠️ PARTIAL | The create flow is implemented, but the project currently has zero discovered tests due to the test-runner configuration. |

### ADR Compliance

| ADR | Constraint | Status | Evidence |
|-----|-----------|--------|----------|
| Pull Request Workflow Client and Auth | Kiota-backed typed Forgejo client and repository-scoped auth | ✅ PASS | The implementation uses the generated Forgejo client and per-request bearer auth in [src/Forgejo.McpServer/Services/ForgejoClientFactory.cs](../../src/Forgejo.McpServer/Services/ForgejoClientFactory.cs) and [src/Forgejo.McpServer/Program.cs](../../src/Forgejo.McpServer/Program.cs). |
| Pull Request Workflow Client and Auth | Do not bypass branch protections or repository policy | ✅ PASS | The service returns structured repository failures rather than claiming success in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |

### Build & Tests

| Command | Result |
|---------|--------|
| `dotnet build .\src\Forgejo.McpServer\Forgejo.McpServer.csproj --nologo` | ✅ PASS |
| `dotnet test .\tests\Forgejo.McpServer.Tests\Forgejo.McpServer.Tests.csproj --nologo` | ❌ FAIL |

> Test output: `Zero tests ran` with exit code 5. The build is good, but the suite is not currently validating the workflow.

## Findings

### Major

- **M-001**: The full pull-request feature scope is still incomplete
  - Expected: The feature spec requires PR creation, comment, and merge flows; User Story 2 and User Story 3 remain in scope in [docs/features/pull-request-workflow/spec.md](spec.md).
  - Found: The current implementation only covers the create-PR path. The task list still shows comment and merge work as open in [docs/features/pull-request-workflow/tasks.md](tasks.md), and there are no comment or merge models/services in the codebase.
  - File: [docs/features/pull-request-workflow/tasks.md](tasks.md), [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs)
  - Suggested fix: Either scope the release to create-PR only or complete the remaining User Story 2 and 3 implementations before claiming full feature compliance.

- **M-002**: The test project is not executing the regression suite
  - Expected: The implementation should be verified by running the TUnit suite so spec and ADR compliance are proven by tests.
  - Found: `dotnet test .\tests\Forgejo.McpServer.Tests\Forgejo.McpServer.Tests.csproj --nologo` exits with `Zero tests ran` and exit code 5.
  - File: [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs)
  - Suggested fix: fix the TUnit discovery/configuration so the test project actually executes before accepting the implementation as verified.

## Next Steps

- Narrow the review scope to the create-PR slice if that is the intended increment.
- Complete the comment and merge story items in [docs/features/pull-request-workflow/tasks.md](tasks.md) before claiming the full PR workflow is ready.
- Fix the test-runner discovery issue so the regression suite can guard the implementation in CI.
