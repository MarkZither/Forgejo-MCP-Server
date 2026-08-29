# Feature Specification: Pull Request Workflow

- **Created on**: 2026-08-30
- **Status**: Draft

## Executive Summary

- **Objective**: Enable a coding agent to create, review, and merge Forgejo pull requests as part of a natural repository workflow.
- **Primary user**: Software engineers using Forgejo-based repositories with GitHub Copilot or similar agentic tooling.
- **Value delivered**: Reduces context switching by letting the user manage review, discussion, and merge decisions from the IDE without manual repository operations.
- **Scope**: Includes PR creation, reviewable metadata, branch association, merge actions, and comment-based collaboration. Excludes branch creation, repository browsing, file editing, release automation, advanced compliance workflow, and repository governance beyond the PR lifecycle.
- **Change type**: additive to existing
- **Describes AI capability**: yes
- **Primary success criterion**: A user can open a valid pull request, contribute review feedback, and complete a merge when repository policy permits it without leaving the editor workflow.

## Non-Scope *(required)*

- Automatic code generation beyond the requested repository workflow
- Custom approval rules beyond the target Forgejo repository configuration
- Full release management and tag publishing
- Branch protection bypass workflows outside the configured repository policy
- Multi-repository orchestration across unrelated projects

## Assumptions

- Developers are working in a single Forgejo repository and have the required authentication scope for repo and PR operations.
- Repository branch protections and merge policies are enforced by the Forgejo instance and are not overridden by the MCP layer.
- A user-facing agent can read repository context before creating a PR but cannot bypass repository permissions or approval policies.
- Standard Forgejo pull request behavior, including merge commit, squash, and rebase workflows, is supported by the underlying instance.

## AI Cost Posture *(required when "Describes AI capability" is "yes"; omit otherwise)*

- **Model-tier commitment**: N/A - model chosen by runtime (GitHub Copilot runtime governs model selection)
- **Latency budget**: N/A - governed by the runtime platform
- **Prompt-stability invariant**: tool schema and repository action contract remain versioned in this repo; changes to the core pull request action vocabulary trigger a spec amendment
- **Per-call cost ceiling**: N/A - billed via the runtime platform plan
- **Cost-incident escalation**: N/A - cost governed by the runtime platform plan

## User Scenarios & Tests *(required)*

### User Story 1 - Create a pull request from a prepared branch change (Priority: P1)

A developer wants to propose a change from an already prepared branch so that reviewers can evaluate the work before merge.

**Why this priority**: This is the core value of the workflow and is the minimum capability needed for reviewable change delivery.

**Independent Test**: The full journey can be validated by preparing a valid branch change, opening a PR, and confirming that the PR appears with the correct metadata.

**Acceptance Scenarios**:

1. **Given** a repository with an active branch and a valid change set, **When** the user requests a pull request, **Then** the system creates a PR that references the source branch and target branch, includes title and description, and exposes it for review.
2. **Given** the target repository does not allow the requested merge path, **When** the user submits a PR, **Then** the system surfaces the blocking condition without creating an invalid PR.

---

### User Story 2 - Review and comment on pull request discussion (Priority: P1)

A developer or reviewer needs to add progress updates or actionable feedback on the PR without leaving the coding workflow.

**Why this priority**: Review collaboration is essential to the PR lifecycle and prevents AI-assisted work from becoming a dead-end after creation.

**Independent Test**: A comment can be added to an existing PR and be visible in the thread without requiring manual UI navigation.

**Acceptance Scenarios**:

1. **Given** an open PR with a valid issue reference, **When** a reviewer comments, **Then** the comment is recorded against the PR and is retrievable by the repository thread.
2. **Given** the user attempts to comment on a PR that does not exist or is inaccessible, **When** the request is made, **Then** the system returns a clear failure result and does not misreport success.

---

### User Story 3 - Merge a validated pull request (Priority: P1)

A maintainer wants to complete a change only after review criteria are satisfied.

**Why this priority**: Merge completion is the business outcome that turns the change into a completed delivery artifact.

**Independent Test**: A valid PR can be merged using the configured repository merge mode and the resulting branch state is updated accordingly.

**Acceptance Scenarios**:

1. **Given** a PR that meets the repository merge policy, **When** the user requests a merge, **Then** the system completes the merge under the configured merge strategy and confirms the result.
2. **Given** a PR that is blocked by merge conflicts or policy checks, **When** the merge is attempted, **Then** the system reports the reason and leaves the PR in a non-merged state.

---

### Edge Cases

- What happens when the source branch has diverged from the target branch?
- How does the system handle a PR title or body that exceeds repository length limits?
- What happens when a PR is already merged or closed before the merge request is executed?
- What happens when a repository requires a review or has branch protection rules that block merge?

### Failure Modes *(include if the feature has external dependencies or shared state)*

- What happens when the Forgejo instance is temporarily unavailable while creating or merging a PR?
- What happens when two users attempt to merge the same PR at nearly the same time?
- What happens when a PR references a branch that was deleted after creation?
- What consistency model is required? The PR state must reflect the latest authoritative Forgejo state before any mutation is attempted.

## Requirements *(required)*

### Functional Requirements

- **FR-001**: The system MUST allow a user to propose a pull request from a valid repository branch with a clear title and overview.
- **FR-002**: The system MUST support attaching a PR to the correct target branch and source branch for the repository context.
- **FR-003**: The system MUST surface the repository’s merge and review requirements before attempting a merge.
- **FR-004**: The system MUST allow a user to add comments to an existing pull request thread.
- **FR-005**: The system MUST allow a user to merge an eligible PR when the repository policy permits it.
- **FR-006**: The system MUST report failures clearly when the PR cannot be created, commented on, or merged due to repository state, permissions, or validation rules.
- **FR-007**: The system MUST avoid claiming success when the Forgejo API indicates a failed or partial operation.
- **FR-008**: The system MUST preserve the PR’s identity and thread history during creation and review operations.

### Key Entities *(include if the feature involves data)*

- **Repository**: A Forgejo project containing source code, branches, and PR history.
- **Pull Request**: A reviewable change proposal that links a source branch to a target branch.
- **Branch**: A versioned line of work that forms the source or target of a PR.
- **Review Comment**: A discussion item attached to a PR thread.
- **Merge Result**: The repository state after a merge operation is completed or rejected.

## Success Criteria *(required)*

### Measurable Outcomes

- **SC-001**: A user can open a pull request for a valid change in under 2 minutes without manual repository navigation.
- **SC-002**: At least 90% of valid PR creation and merge attempts complete successfully when the repository policy allows them.
- **SC-003**: Review comments are attached to the correct PR and remain discoverable in repository discussion history.
- **SC-004**: Repository policy failures are shown before merge so users can resolve blockers without repeated attempts.

## Conformance Criteria *(required)*

### Conformance Cases

| ID | Scenario | Input | Expected Output |
|----|----------|-------|-----------------|
| CC-001 | Happy path PR creation | Valid source branch and target branch in a writable repository | A PR is created with the provided title, description, and branch linkage |
| CC-002 | PR comment creation | Existing PR ID and a valid comment body | Comment is attached to the PR thread and visible in the discussion |
| CC-003 | Merge success | PR eligible under repository policy and merge request is valid | PR is merged with the configured repository behavior and the result is confirmed |
| CC-004 | Policy failure | PR blocked by branch protection or merge constraints | The system reports the policy reason and does not report success |
| CC-005 | Must NOT happen | User requests a merge for a non-existent or closed PR | The system must NOT report a successful merge or mutate repository state |

## Invariants

- Each PR must be tied to exactly one source branch and one target branch in the same repository.
- A merge must never be reported as successful unless the Forgejo API confirms the repository state changed accordingly.
- Review comments must remain attributable to the correct PR and user context.
- The system must never bypass configured repository policy in order to force a merge.

## Compatibility and Transition *(required when Change type is not "new surface")*

- **Existing consumers**: Forgejo repositories, review workflows, existing CI automation, and repository maintainers using the PR lifecycle
- **Backward compatibility stance**: strict
- **Coexistence requirement**: N/A for the initial scope; no prior PR workflow contract is being replaced
- **Rollback requirement**: If this feature is disabled, repository workflows continue under the underlying Forgejo system without data loss or forced migration
- **Deprecation signal**: N/A for a new additive capability
- **Telemetry requirement**: log structured action outcome for PR create, comment, and merge attempts to support operational diagnosis

## Related Specs

- [PR feature hierarchy](../envisioning/structure.md) - product-level grouping for this workflow

## Spec Evolution Log *(required)*

| Version | Date | Change Summary | Trigger | Author |
|---------|------|----------------|---------|--------|
| 1.0 | 2026-08-30 | Initial feature draft for pull request workflow | new feature spec | Copilot |
