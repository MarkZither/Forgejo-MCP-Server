#!/usr/bin/env pwsh
Write-Host ">>> Husky pre-commit.ps1 is running" -ForegroundColor Cyan
dotnet husky run --group pre-commit
