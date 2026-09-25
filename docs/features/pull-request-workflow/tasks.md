# Tasks: Pull Request Workflow

## Setup

- [x] [P] Confirm the Forgejo API contract, auth scopes, and version assumptions for PR operations and document them in the feature boundary.
- [x] [P] Validate that the branching strategy and repo governance follow the SDD process and keep work on the feature branch.
- [x] [P] Capture the design decision for the Kiota-backed Forgejo client and auth model in an ADR if the client strategy is not already approved.

## Foundational

- [x] [P] Create the Kiota client skeleton and shared HTTP configuration for Forgejo repository operations.
- [x] [P] Define the shared repository and PR domain models needed to create, read, and merge pull requests.
- [x] [P] Add the base service layer that maps Forgejo API responses to repository-friendly PR operations.
- [x] [P] Implement the error and policy handling path so failed PR operations surface actionable repository reasons.

## User Story 1 - Create a pull request from a branch change (Priority: P1)

Current repo status: the request contract, validation rules, and MCP tool surface are implemented. The remaining work is the live Forgejo API call, repository lookup/branch validation, and end-to-end verification against a real repository.

### Models

- [x] [P] Add structural models for pull request creation payloads and returned PR metadata.
- [x] [P] Add validation rules for source branch, target branch, title, and body constraints.

### Services

- [x] Implement the service method that creates a pull request for a valid repository branch pair.
- [x] Implement repository lookup and branch validation before the PR creation call is attempted.

### Endpoints

- [x] Add the Forgejo PR create endpoint call and map the raw API result into a typed result model.
- [x] Ensure the endpoint surfaces repository policy blockers and duplicate state issues without claiming a false success.

### Integration

- [ ] Validate the end-to-end create-PR flow using a test repository and confirm the PR is visible with correct metadata.
- [ ] Verify the user experience fails cleanly when branch permissions or repository rules reject the request.

## User Story 2 - Review and comment on pull request discussion (Priority: P1)

### Models

- [ ] [P] Define the comment payload and comment thread model for PR discussion items.
- [ ] [P] Add retrieval models for existing PR review comments and conversation history.

### Services

- [ ] Implement the service method for adding comments to a PR thread.
- [ ] Implement the service method for fetching the latest thread and PR state needed for review context.

### Endpoints

- [ ] Add the Forgejo endpoint calls for listing and creating review comments on a PR.
- [ ] Ensure review comments are associated with the correct repository and PR identity.

### Integration

- [ ] Validate that comments appear in the correct PR thread and can be retrieved later in the workflow.
- [ ] Confirm that invalid PR IDs or inaccessible repositories return clear, non-success errors.

## User Story 3 - Merge a validated pull request (Priority: P1)

### Models

- [ ] [P] Define merge request and merge result models for configured merge strategies.
- [ ] [P] Add models for conflict and policy rejection states returned by the Forgejo API.

### Services

- [ ] Implement the service method that triggers a merge when repository policies allow it.
- [ ] Implement validation of PR state before merge to avoid mutating closed, merged, or blocked PRs.

### Endpoints

- [ ] Add the merge endpoint call and map API errors into a user-safe failure model.
- [ ] Confirm the merge strategy selected by the repository is respected and returned in the result metadata.

### Integration

- [ ] Validate merge success on a valid PR and confirm repository state changes accordingly.
- [ ] Validate merge rejection when branch protection, conflicts, or policy checks block the request.

## Polish

- [ ] [P] Review the generated tool surface and ensure each action is exposed as a clear, permission-aware MCP command.
- [ ] [P] Validate naming consistency across the client library and MCP tool layer.
- [ ] [P] Confirm the branch, spec, and implementation state match the devsquad workflow before moving to PR preparation.
- [ ] [P] Record any open risks, follow-up items, and security considerations for the next implementation increment.
