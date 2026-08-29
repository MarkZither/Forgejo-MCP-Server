# Feature Specification: Issue Management

- **Created on**: 2026-08-30
- **Status**: Draft

## Executive Summary

- **Objective**: Allow a coding agent to create, read, and comment on Forgejo issues as part of an engineering workflow.
- **Primary user**: Engineers, maintainers, and project leads who use Forgejo for task tracking and collaboration.
- **Value delivered**: Keeps issue discussions and task tracking accessible inside the same toolchain used for repository work and PR review.
- **Scope**: Includes issue creation, issue listing, issue lookup, and comment creation on issue threads. Excludes PR lifecycle automation, release creation, and cross-project workflow orchestration beyond the repository scope.
- **Change type**: additive to existing
- **Describes AI capability**: yes
- **Primary success criterion**: A user can create or reference a valid issue and exchange review feedback without leaving the editor workflow.

## Non-Scope *(required)*

- Pull request creation or merge actions
- Release generation
- Full project management beyond repository issue lifecycle
- Bulk issue migration or administration across unrelated repositories

## Assumptions

- The repository has issue tracking enabled and the current user has permission to read or create issues.
- The Forgejo issue model includes basic issue metadata such as title, body, labels, and comments.
- The user is operating within a single repository context and not across multiple independent issue trackers.
- Issue comments are for repository discussion and are not treated as approval decisions by default.

## AI Cost Posture *(required when "Describes AI capability" is "yes"; omit otherwise)*

- **Model-tier commitment**: N/A - model chosen by runtime (GitHub Copilot runtime governs model selection)
- **Latency budget**: N/A - governed by the runtime platform
- **Prompt-stability invariant**: issue-tool names and returned metadata remain versioned in this repo; changes to the issue action contract trigger a spec amendment
- **Per-call cost ceiling**: N/A - billed via the runtime platform plan
- **Cost-incident escalation**: N/A - cost governed by the runtime platform plan

## User Scenarios & Tests *(required)*

### User Story 1 - Create a repository issue (Priority: P1)

A developer or maintainer wants to record a task or bug in the repository so that it is visible to the team.

**Why this priority**: Issue creation is the primary workflow for capturing work and open questions.

**Independent Test**: A valid issue can be created with a title and description and then retrieved in the repo issue list.

**Acceptance Scenarios**:

1. **Given** a valid repository and required metadata, **When** the user creates an issue, **Then** the system records the issue and returns the created issue identity and metadata.
2. **Given** an invalid issue payload or lack of permission, **When** the creation request is made, **Then** the system surfaces the error without claiming a successful issue was created.

---

### User Story 2 - Read and list issues (Priority: P1)

A user needs the current issue state before implementing or discussing work.

**Why this priority**: Issue discovery is necessary for context before changes and reviews.

**Independent Test**: The user can request a list of issues or read a specific issue and receive accurate repository data.

**Acceptance Scenarios**:

1. **Given** a repository with active issues, **When** the user lists issues, **Then** the system returns the relevant issue metadata in a valid order or filter.
2. **Given** an issue ID does not exist or is inaccessible, **When** the read is requested, **Then** the system returns a clear error rather than fabricated issue content.

---

### User Story 3 - Comment on issues (Priority: P1)

A reviewer or maintainer needs to add notes to an issue thread for clarity or follow-up.

**Why this priority**: Collaboration on issue discussions is a core part of the engineering workflow.

**Independent Test**: A comment can be posted to a valid issue and appears in the thread.

**Acceptance Scenarios**:

1. **Given** a valid issue and comment payload, **When** the user posts a comment, **Then** the comment is associated with the issue and returned as a visible thread item.
2. **Given** the issue is missing or inaccessible, **When** the comment is attempted, **Then** the system returns a clear failure and does not misreport success.

---

### Edge Cases

- What happens if an issue has a very long body or title?
- How does the system behave when labels or assignees are not supported by the repository configuration?
- What happens when the issue is closed or locked before a comment is added?

### Failure Modes *(include if the feature has external dependencies or shared state)*

- What happens when the Forgejo API is unavailable during issue creation or read attempts?
- What happens when two users create or comment on the same issue simultaneously?
- What consistency model is required? Issue state must reflect the latest repository truth and comments must be attributable to the correct issue and user context.

## Requirements *(required)*

### Functional Requirements

- **FR-001**: The system MUST allow a user to create a valid repository issue with a title and descriptive body.
- **FR-002**: The system MUST allow a user to list issues and read a specific issue for the selected repository.
- **FR-003**: The system MUST allow a user to add a comment to an existing issue thread.
- **FR-004**: The system MUST validate issue data and permission before attempting creation or comment updates.
- **FR-005**: The system MUST report inaccessible or missing issues clearly and avoid claiming a successful action when the repository rejects it.

### Key Entities *(include if the feature involves data)*

- **Repository**: A Forgejo project with issue tracking enabled.
- **Issue**: A repository task or bug record with status and metadata.
- **Issue Comment**: A discussion item attached to an issue thread.
- **Issue State**: The current issue lifecycle state as returned by the repository.

## Success Criteria *(required)*

### Measurable Outcomes

- **SC-001**: A user can create or read a repository issue in under 2 minutes without manual UI navigation.
- **SC-002**: Issue creation and comments are associated with the correct repository and issue thread.
- **SC-003**: Invalid or inaccessible issues produce clear failure states rather than false success.

## Conformance Criteria *(required)*

### Conformance Cases

| ID | Scenario | Input | Expected Output |
|----|----------|-------|-----------------|
| CC-001 | Issue creation | Valid repository, title, and body | A new issue is created and returned with repository metadata |
| CC-002 | Issue listing | Valid repository | Relevant issue metadata is returned |
| CC-003 | Comment creation | Valid issue and comment body | Comment is added to the issue thread |
| CC-004 | Must NOT happen | Invalid issue ID or inaccessible repo | The system must NOT report success or mutate issue state |

## Invariants

- Issue creation and comment operations must always respect repository permissions and issue existence.
- The system must not claim success for issue updates unless the Forgejo API confirms the repository state changed accordingly.
- Issue comments must remain attributable to the correct issue and user context.

## Compatibility and Transition *(required when Change type is not "new surface")*

- **Existing consumers**: repository maintainers, Copilot-style assistants, and users who rely on issue tracking for work planning
- **Backward compatibility stance**: strict
- **Coexistence requirement**: N/A for a new additive capability
- **Rollback requirement**: No migration required; disabling the feature simply removes the issue helper surface
- **Deprecation signal**: N/A for a new additive capability
- **Telemetry requirement**: log create, read, and comment outcomes for issue workflow visibility

## Related Specs

- [Repository Browsing](../repository-browsing/spec.md) - provides repo context for issue selection
- [Pull Request Workflow](../pull-request-workflow/spec.md) - links issue context to review and merge activity

## Spec Evolution Log *(required)*

| Version | Date | Change Summary | Trigger | Author |
|---------|------|----------------|---------|--------|
| 1.0 | 2026-08-30 | Initial feature draft for issue management | new feature spec | Copilot |
