# Conventional Commits

This project follows the [Conventional Commits](https://www.conventionalcommits.org/) specification.

## Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

## Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- **refactor**: A code change that neither fixes a bug nor adds a feature
- **perf**: A code change that improves performance
- **test**: Adding missing tests or correcting existing tests
- **build**: Changes that affect the build system or external dependencies
- **ci**: Changes to CI configuration files and scripts
- **chore**: Other changes that don't modify src or test files
- **revert**: Reverts a previous commit

## Scope

Optional scope indicates the area of the codebase affected:
- `core`: Core library changes
- `console`: Console application changes
- `winforms`: WinForms application changes
- `tests`: Test changes
- `docs`: Documentation changes
- `ci`: CI/CD changes
- `build`: Build system changes

## Examples

### Feature
```
feat(winforms): add version check functionality

Adds automatic version checking and update notification to the WinForms installer.
```

### Bug Fix
```
fix(core): handle network errors during download

Fixes issue where download would fail silently on network errors. Now shows proper error messages.
```

### Documentation
```
docs: update installation instructions

Updates README with new prerequisites and installation steps.
```

### Refactor
```
refactor(core): extract installation logic to service

Moves installation logic from Program.cs to InstallationService for better testability.
```

### Test
```
test(core): add tests for InstallationService

Adds unit tests for download and installation methods.
```

### Build
```
build: update .NET SDK to 8.0

Updates project files to use .NET 8.0 SDK.
```

### CI
```
ci: add GitHub Actions workflow

Adds CI workflow for build, test, and Docker image creation.
```

## Breaking Changes

Use `!` after the type/scope to indicate breaking changes:

```
feat(api)!: change method signature

BREAKING CHANGE: Method now requires additional parameter.
```

## Footer

Use footer for:
- Breaking changes: `BREAKING CHANGE: <description>`
- Issue references: `Closes #123`, `Fixes #456`
- Co-authors: `Co-authored-by: Name <email>`

## Best Practices

1. Use present tense ("add feature" not "added feature")
2. Use imperative mood ("move code" not "moves code")
3. Don't capitalize the first letter
4. No period at the end of the subject line
5. Limit subject line to 72 characters
6. Reference issues and pull requests in the footer

## Commit Message Examples

```
feat(winforms): add auto-update functionality

Implements automatic update checking and installation for Claude Code.
Checks for updates on startup and notifies user if newer version is available.

Closes #42
```

```
fix(core): fix download progress reporting

Fixes issue where download progress was not reported correctly for files
without Content-Length header.

Fixes #38
```

```
docs: add testing guide

Adds comprehensive testing guide with examples and best practices.
```
