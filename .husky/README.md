# Husky.NET Setup for Forgejo-MCP-Server

This repository uses [Husky.NET](https://github.com/alirezanet/Husky.Net) for Git hooks to enforce code quality standards.

## Pre-commit Hook

The pre-commit hook automatically runs before each commit to ensure code formatting standards are met.

### What it does

1. Detects staged C# files
2. Runs `dotnet format --verify-no-changes` to check formatting
3. Blocks the commit if formatting violations are found

### If the pre-commit hook fails

Run the following command to auto-format your code:

```powershell
dotnet format
```

Then re-stage your changes and commit again:

```powershell
git add .
git commit -m "your message"
```

### Installing Husky.NET (for new clones)

When setting up the repository for the first time in a .NET project:

1. Install Husky.NET as a local tool:
   ```powershell
   dotnet new tool-manifest
   dotnet tool install Husky
   ```

2. Install the Git hooks:
   ```powershell
   dotnet husky install
   ```

3. The `.husky/pre-commit` script will now run automatically on every commit.

### Bypassing the hook (emergency only)

If you absolutely must commit without running the hook:

```powershell
git commit --no-verify -m "your message"
```

**Note:** This should only be used in emergencies. All code should pass formatting checks before being committed.

## EditorConfig

The repository includes a `.editorconfig` file that defines K&R styling rules for C#. Your IDE should automatically apply these rules as you type.

### IDE Setup

- **Visual Studio**: EditorConfig is supported natively
- **VS Code**: Install the "EditorConfig for VS Code" extension
- **Rider**: EditorConfig is supported natively

All three IDEs will automatically format code according to `.editorconfig` when you save files.
