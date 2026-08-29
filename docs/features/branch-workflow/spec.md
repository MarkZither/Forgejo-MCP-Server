# Feature Specification: Branch Workflow

- **Created on**: 2026-08-30
- **Status**: Draft

## Executive Summary

- **Objective**: Enable a coding agent to inspect, create, and manage Forgejo branches as part of a safe repository workflow.
- **Primary user**: Software engineers and maintainers working on feature work from repository branches.
- **Value delivered**: Makes branch handling predictable and traceable without requiring manual navigation through the Forgejo UI.
- **Scope**: Includes branch listing, branch creation, and branch selection for ongoing work. Excludes repository browsing, file editing, PR lifecycle automation, and release creation.
- **Change type**: additive to existing
- **Describes AI capability**: yes
- **Primary success criterion**: A user can create or select a valid branch for a change without losing repo context or creating unclear branch state.

## Non-Scope *(required)*

- File editing or commit creation
- Pull request creation or merge enforcement
- Repository or branch deletion beyond explicitly requested admin actions
- Release generation
- Cross-repository branch synchronization

## Assumptions

- The user has valid access to the repository and branch operations in the configured Forgejo context.
- The repository has a valid default branch and branch naming conventions are enforced by the instance or team policy.
- The MCP layer must follow the repository’s configured permissions rather than bypass branch protections.
- Branch actions are performed within one repository at a time.

## AI Cost Posture *(required when "Describes AI capability" is "yes"; omit otherwise)*

- **Model-tier commitment**: N/A - model chosen by runtime (GitHub Copilot runtime governs model selection)
- **Latency budget**: N/A - governed by the runtime platform
- **Prompt-stability invariant**: branch operation tool names and result states remain versioned in this repo; changes to branch action vocabulary trigger a spec amendment
- **Per-call cost ceiling**: N/A - billed via the runtime platform plan
- **Cost-incident escalation**: N/A - cost governed by the runtime platform plan

## User Scenarios & Tests *(required)*

### User Story 1 - Create a branch for a feature change (Priority: P1)

A developer needs a clean working branch before making a repository change.

**Why this priority**: Safe branch creation is foundational to the rest of the coding workflow.

**Independent Test**: A valid branch can be created from a known source branch and then confirmed in the repository state.

**Acceptance Scenarios**:

1. **Given** a valid repository and source branch, **When** the user requests a new branch, **Then** the system creates the branch from the chosen source without modifying unrelated repository state.
2. **Given** the requested branch name is invalid or already exists, **When** the creation request is made, **Then** the system returns a clear failure reason and does not claim success.

---

### User Story 2 - List and select repository branches (Priority: P1)

A developer wants to understand the current branch set and choose the correct branch for a task.

**Why this priority**: Branch discovery prevents work from being started on the wrong branch or target.

**Independent Test**: The user can list branches and select the correct active or source branch for the task.

**Acceptance Scenarios**:

1. **Given** a repository with multiple branches, **When** the user requests the branch list, **Then** the system returns the available branch metadata and names.
2. **Given** the requested branch does not exist, **When** the selection is attempted, **Then** the system returns a clear not-found result.

---

### User Story 3 - Switch to a valid working branch (Priority: P2)

A user wants to put new work on a branch that matches the task context and repository policy.

**Why this priority**: Branch switching is the workflow glue, but it depends on valid branch existence and repo access.

**Independent Test**: The system can confirm the current working branch and validate whether the target branch is a valid switch target.

**Acceptance Scenarios**:

1. **Given** a valid branch target, **When** the user requests to work on it, **Then** the system confirms the target branch state and moves the context to the correct branch.
2. **Given** the target branch is missing or inaccessible, **When** the branch change is attempted, **Then** the system reports the reason without silently switching.

---

### Edge Cases

- What happens when the requested branch name violates repository naming rules?
- How is the system expected to behave when the source branch has no valid head revision?
- What happens when the repository has a default branch but the target branch does not yet exist?

### Failure Modes *(include if the feature has external dependencies or shared state)*

- What happens when the Forgejo API is unavailable while listing or creating branches?
- What happens when two users create the same branch name at nearly the same time?
- What consistency model is required? Branch state must reflect the current repository truth and should not be assumed without API confirmation.

## Requirements *(required)*

### Functional Requirements

- **FR-001**: The system MUST allow a user to list the available branches for a valid repository.
- **FR-002**: The system MUST allow a user to create a new working branch from an existing repository branch.
- **FR-003**: The system MUST validate that the source branch exists and the target branch name is valid before creating it.
- **FR-004**: The system MUST return a clear failure result when the branch already exists, is invalid, or cannot be created under repository policy.
- **FR-005**: The system MUST keep branch operations scoped to the selected repository and not mutate unrelated systems.

### Key Entities *(include if the feature involves data)*

- **Repository**: A Forgejo project containing branches and branch history.
- **Branch**: A named line of work associated with a repository state.
- **Source Branch**: The branch used as the starting point for a new branch.
- **Branch State**: The current branch identity and metadata returned by the repository.

## Success Criteria *(required)*

### Measurable Outcomes

- **SC-001**: A user can create or select a branch in under 2 minutes without manual repository navigation.
- **SC-002**: Invalid branch requests are rejected clearly and do not leave ambiguous repository state.
- **SC-003**: The selected repository branch matches the intended working context for subsequent task steps.

## Conformance Criteria *(required)*

### Conformance Cases

| ID | Scenario | Input | Expected Output |
|----|----------|-------|-----------------|
| CC-001 | Branch listing | Valid repository | The available branches are returned with correct metadata |
| CC-002 | Branch creation | Valid repository and valid source branch | A new branch is created from the chosen source |
| CC-003 | Invalid branch name | Duplicate or malformed target name | A clear validation or repository policy failure is returned |
| CC-004 | Must NOT happen | User requests a branch action for a repo without access | The system must NOT mutate branch state or claim success |

## Invariants

- A new branch must always originate from an existing repository branch.
- The system must not claim success for branch creation unless the repository confirms the branch exists.
- Branch naming and permission validation must be enforced before the action is executed.

## Compatibility and Transition *(required when Change type is not "new surface")*

- **Existing consumers**: repository users, Copilot-style agents, and workers creating task-specific branch contexts
- **Backward compatibility stance**: strict
- **Coexistence requirement**: N/A for a new additive capability
- **Rollback requirement**: No migration is needed; disabling the feature simply removes branch helper actions
- **Deprecation signal**: N/A for a new additive capability
- **Telemetry requirement**: log successful branch creation and validation failures for operational review

## Related Specs

- [Repository Browsing](../repository-browsing/spec.md) - provides the repository and path context needed to choose a branch
- [Pull Request Workflow](../pull-request-workflow/spec.md) - depends on a valid branch being available before PR creation

## Spec Evolution Log *(required)*

| Version | Date | Change Summary | Trigger | Author |
|---------|------|----------------|---------|--------|
| 1.0 | 2026-08-30 | Initial feature draft for branch workflow | new feature spec | Copilot |
