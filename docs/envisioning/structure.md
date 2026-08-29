# Structure (Cache)

> Source of truth: Board. Check board for current state.  
> Platform configured in: .memory/board-config.md

## Epic and Feature Hierarchy

| ID | Type | Name | Parent | Priority |
|----|------|------|--------|----------|
| #1 | Epic | Forgejo MCP Server | - | - |
| #2 | Feature | Repository discovery and browsing | #1 | P1 |
| #3 | Feature | File read and write operations | #1 | P1 |
| #4 | Feature | Branch and repository workflow management | #1 | P1 |
| #5 | Feature | Issue management and comments | #1 | P1 |
| #6 | Feature | Pull request creation and merge workflow | #1 | P1 |
| #7 | Feature | Release and tag generation | #1 | P2 |
| #8 | Feature | Authentication, permissions, and resilience | #1 | P1 |

## Dependency Map

- #3 depends on #2
- #4 depends on #2
- #5 depends on #2
- #6 depends on #3, #4, and #5
- #7 depends on #6
- #8 is a cross-cutting dependency for all features

## Rationale

A single epic is the right structure because all capabilities are parts of the same Forgejo automation layer. The features all share the same underlying service, authentication model, and Copilot tool surface and are intended to be delivered as one coherent developer workflow rather than as separate products.

The main MVP flow is:
1. discover repository context
2. read and edit files
3. create a branch and work on a change
4. create or review issues and pull requests
5. merge and optionally publish a release

## MVP Priority

### P1
- Repository discovery and browsing
- File read and write operations
- Branch and repository workflow management
- Issue management and comments
- Pull request creation and merge workflow
- Authentication, permissions, and resilience

### P2
- Release and tag generation

## Handoff Envelope

- Product framing: [docs/envisioning/README.md](README.md)
- Source of truth: board or project backlog
- Assumption: the first release focuses on the core developer workflow and defers advanced release automation until after the initial MCP integration is proven
- Pending prioritization: release automation and broader Forgejo project support can be expanded after the branch, file, and PR workflow is stable
