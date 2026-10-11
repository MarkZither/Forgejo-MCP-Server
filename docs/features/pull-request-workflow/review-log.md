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

---

# Review: Pull Request Workflow

**Date**: 2026-10-11
**Validated artifacts**: [docs/features/pull-request-workflow/spec.md](spec.md), [docs/architecture/decisions/pr-workflow-client-auth.md](../../architecture/decisions/pr-workflow-client-auth.md)
**Reviewed code**: [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs), [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs), [docs/features/pull-request-workflow/tasks.md](tasks.md)

## Result

**Status**: PASSED_WITH_FINDINGS
- Critical: 0
- Major: 2
- Minor: 0

## Checklist

### Spec Compliance

| ID | Requirement | Status | Evidence |
|----|-------------|--------|----------|
| FR-001 | Create PR from valid source and target branches | ✅ PASS | Create flow is implemented and validated by integration and regression tests in [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs). |
| FR-004 | Add PR discussion comments | ✅ PASS | Comment create and retrieval flows are implemented and tested in [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs). |
| FR-005 | Merge eligible PRs | ✅ PASS | Merge flow is implemented with successful and blocked merge regression tests in [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs). |
| FR-006 | Report failures clearly | ✅ PASS | Structured error codes are mapped for create, comment, and merge paths in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |
| FR-003 | Surface merge requirements before attempting merge | ❌ FAIL | Merge flow checks only merged or closed state before calling merge endpoint, with no explicit policy/readiness check prior to mutation attempt at [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs#L196) and [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs#L204). |

### Conformance Test Mapping

| CC-ID | Scenario | Test Case | Status |
|------|----------|-----------|--------|
| CC-001 | Happy path PR creation | `ForgejoPullRequestService_Validation_CreatePullRequest_UsesConfiguredRepo` | ✅ Mapped |
| CC-002 | PR comment creation | `ForgejoPullRequestService_AddComment_RecordsDiscussionComment` | ✅ Mapped |
| CC-003 | Merge success | `ForgejoPullRequestService_Merge_MergesSuccessfulPullRequest` | ✅ Mapped |
| CC-004 | Policy failure | `ForgejoPullRequestService_Merge_RejectsBlockedPullRequest`, `ForgejoPullRequestService_CreatePullRequest_ReportsRepositoryPolicyFailureCleanly` | ✅ Mapped |
| CC-005 | Non-existent or closed PR merge must not succeed | No explicit regression test found | ❌ Missing |

### ADR Compliance

| ADR | Constraint | Status | Evidence |
|-----|-----------|--------|----------|
| Pull Request Workflow Client and Auth | Kiota-backed typed client | ✅ PASS | Service uses Kiota generated client abstractions through repository and pull request builders in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |
| Pull Request Workflow Client and Auth | Preserve repository policy and fail clearly | ✅ PASS | Create and merge policy failures return structured non-success results instead of claimed success in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |

### Build and Tests

| Command | Result |
|---------|--------|
| `dotnet test tests/Forgejo.McpServer.Tests/Forgejo.McpServer.Tests.csproj` | ✅ PASS (50 succeeded, 0 failed) |

## Findings

### Major

- **M-003**: Merge readiness is not evaluated before merge mutation attempt
  - Expected: FR-003 requires merge and review requirements to be surfaced before merge attempt.
  - Found: The merge path only checks `Merged` and `State == closed`, then immediately submits merge request.
  - File: [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs#L196), [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs#L204)
  - Suggested fix: Evaluate mergeability preconditions from PR metadata before posting merge, and return a policy or readiness failure when prerequisites are not met.

- **M-004**: Missing conformance regression for non-existent or closed PR merge denial
  - Expected: CC-005 requires explicit proof that merge cannot succeed for non-existent or closed pull requests.
  - Found: Merge test coverage includes success and policy-blocked conflict, but no dedicated merge non-existent or closed-state regression.
  - File: [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs#L261), [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs#L281)
  - Suggested fix: Add regressions for merge not found and closed PR merge attempts that assert non-success and stable repository state.

## Next Steps

- Add pre-merge readiness checks that map to FR-003 before invoking the merge endpoint.
- Add dedicated CC-005 regressions for both non-existent and closed pull request merge attempts.

---

# Review: Pull Request Workflow

**Date**: 2026-10-11
**Validated artifacts**: [docs/features/pull-request-workflow/spec.md](spec.md), [docs/architecture/decisions/pr-workflow-client-auth.md](../../architecture/decisions/pr-workflow-client-auth.md), [docs/features/pull-request-workflow/tasks.md](tasks.md)
**Reviewed code**: [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs), [src/Forgejo.McpServer/Tools/ForgejoPullRequestTools.cs](../../src/Forgejo.McpServer/Tools/ForgejoPullRequestTools.cs), [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs)

## Result

**Status**: PASSED
- Critical: 0
- Major: 0
- Minor: 0

## Checklist

### Spec Compliance

| ID | Requirement | Status | Evidence |
|----|-------------|--------|----------|
| FR-001 | Create PR from valid branch change | ✅ PASS | Create flow is implemented and validated in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs) and [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs). |
| FR-003 | Surface merge and review requirements before merge | ✅ PASS | Merge readiness is checked before mutation in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |
| FR-004 | Add PR discussion comments | ✅ PASS | Comment creation and retrieval flows are implemented and covered by tests in [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs). |
| FR-005 | Merge eligible PRs | ✅ PASS | Merge success and failure cases are covered and return structured non-success results in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |
| FR-006 | Report failures clearly | ✅ PASS | Repository policy and validation failures are mapped to structured error codes in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |
| CC-005 | Non-existent or closed PR merge must not succeed | ✅ PASS | Regression coverage verifies missing and closed PR denial in [tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs](../../tests/Forgejo.McpServer.Tests/ForgejoPullRequestFoundationTests.cs). |

### ADR Compliance

| ADR | Constraint | Status | Evidence |
|-----|-----------|--------|----------|
| Pull Request Workflow Client and Auth | Kiota-backed typed Forgejo client and repository-scoped auth | ✅ PASS | Client construction and request pipeline remain centralized in [src/Forgejo.McpServer/Services/ForgejoClientFactory.cs](../../src/Forgejo.McpServer/Services/ForgejoClientFactory.cs) and [src/Forgejo.McpServer/Program.cs](../../src/Forgejo.McpServer/Program.cs). |
| Pull Request Workflow Client and Auth | Do not bypass branch protections or repository policy | ✅ PASS | Merge and create operations fail clearly when repository rules reject the action in [src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs](../../src/Forgejo.McpServer/Services/ForgejoPullRequestService.cs). |

### Build & Tests

| Command | Result |
|---------|--------|
| `dotnet run --project tests/Forgejo.McpServer.Tests/Forgejo.McpServer.Tests.csproj -- --progress off --results-directory TestResults` | ✅ PASS (53 succeeded, 0 failed) |

## Findings

No critical, major, or minor findings remain after the merge-readiness fix.

## Next Steps

- Proceed with the feature handoff or PR review.
- Keep the regression suite in place to guard CC-005 and merge-policy behavior as the workflow evolves.
