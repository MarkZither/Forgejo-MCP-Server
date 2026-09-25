@{
    # Strict linting everywhere except excluded paths
    Severity = @('Error', 'Warning')

    # Ignore Husky folder entirely (bash scripts, no extensions, etc.)
    ExcludePaths = @('.husky')

    # Ignore files that are clearly bash/sh based on shebang
    # PSScriptAnalyzer does not read shebangs, but editors do.
    # This prevents editors from feeding these files to the analyzer.
    ExcludeByRegex = @(
        '.*\.sh$',                # Any .sh file
        '.*#!/usr/bin/env\s+sh',  # Shebang-based detection
        '.*#!/usr/bin/env\s+bash'
    )

    # Keep strict rules everywhere else
    Rules = @{
        PSAvoidUsingCmdletAliases = @{ Enable = $true }
        PSUseDeclaredVarsMoreThanAssignments = @{ Enable = $true }
        PSUseApprovedVerbs = @{ Enable = $true }
        PSUseConsistentWhitespace = @{ Enable = $true }
        PSUseConsistentIndentation = @{ Enable = $true }
        PSUseCorrectCasing = @{ Enable = $true }
        PSUseBOMForUnicodeEncodedFile = @{ Enable = $true }
        PSUseShouldProcessForStateChangingFunctions = @{ Enable = $true }
        PSUseSupportsShouldProcess = @{ Enable = $true }
        PSUseOutputTypeCorrectly = @{ Enable = $true }
        PSUseCompatibleSyntax = @{ Enable = $true }
    }
}
