# Contributing to Forgejo-MCP-Server

Thank you for your interest in contributing to Forgejo-MCP-Server. This document provides guidelines for contributing to the project.

## Getting Started

### Before You Start

**Submit issues first and Fork and submit PR**

Please open an issue to discuss the change you wish to make before starting work. This helps avoid duplicate efforts and ensures your contribution aligns with the project's goals.

### Development Workflow

1. Fork the repository
2. Create a feature branch from `main`
3. Make your changes
4. Ensure all tests pass
5. Submit a pull request

## Code Style

We follow `dotnet format with editorconfig k+r styling as the baseline`. Please ensure your code adheres to these standards before submitting.

To format your code:
```bash
dotnet format
```

The project uses an `.editorconfig` file to enforce K&R styling conventions.

## Testing Requirements

- **All new features must include unit tests**
- **Run existing tests before submitting**

Before submitting your pull request, verify that all tests pass:
```bash
dotnet test
```

## Pull Request Process

1. Ensure your code follows the project's code style
2. Update documentation to reflect any changes
3. Add or update tests as needed
4. Ensure all tests pass
5. Submit your pull request with a clear description of the changes

### Pull Request Checklist

- [ ] Code follows project style guidelines
- [ ] Tests have been added or updated
- [ ] All tests pass
- [ ] Documentation has been updated
- [ ] Commit messages are clear and descriptive

## Questions or Need Help?

If you have questions or need assistance, please open an issue for discussion.

Thank you for contributing to Forgejo-MCP-Server!
