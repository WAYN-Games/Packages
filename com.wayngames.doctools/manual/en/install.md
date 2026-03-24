# Installation

This page covers installing DocTools and its dependencies.

## Prerequisites

| Requirement | Version | How to check |
|-------------|---------|-------------|
| **Bash** | Any (Git Bash on Windows) | `bash --version` |
| **.NET SDK** | 9.0+ | `dotnet --version` |
| **DocFX** | Latest | `docfx --version` |

## Install .NET SDK

Download and install the [.NET SDK](https://dotnet.microsoft.com/download) (version 9.0 or later). DocFX requires it for C# metadata extraction.

## Install DocFX

Install DocFX as a global .NET tool:

```bash
dotnet tool install -g docfx
```

Verify it is on your PATH:

```bash
docfx --version
```

## Install DocTools

Clone the DocTools repository:

```bash
git clone <your-doctools-repo-url> /path/to/DocTools
```

No additional installation is needed. All commands run directly via `doctools.sh`.

> [!TIP]
> Add the DocTools directory to your `PATH` so you can call `doctools.sh` from anywhere:
> ```bash
> export PATH="$PATH:/path/to/DocTools"
> ```

## Verify Installation

Run `doctools.sh` with no arguments to see the usage message:

```bash
doctools.sh
```

You should see the list of available commands (init, build, translate, etc.).

## Next Step

Continue to the [Quickstart](quickstart.md) to initialize documentation for a package and build your first site.
