# DocTools

**DocTools** is a Bash CLI that builds multi-version, multilingual [DocFX](https://dotnet.github.io/docfx/) documentation sites for Unity packages. Point it at a package with a `package.json`, run one command, and get a complete HTML site with API reference, version switcher, language switcher, dark/light mode, and Mermaid diagram support.

## When to Use DocTools

Use DocTools when you need:

- A documentation site for a Unity package with C# API reference
- Multi-version output with a version dropdown for readers
- Automatic translation support with per-language builds
- A polished theme with dark mode, accessibility, and diagram support
- CI/CD deployment to GitHub Pages

DocTools is **not** for projects without a `package.json`, general-purpose static site generation, or documentation that does not need DocFX's C# metadata extraction.

## Key Features

- **Convention over configuration** — reads `package.json` for title, version, and author. No config file needed.
- **One-command builds** — `doctools.sh build` handles metadata extraction, content processing, and output generation
- **Multi-version output** — each version in its own directory with a global version dropdown
- **Multilingual support** — translate docs to 15 languages with hash-based change tracking and a language switcher
- **Custom template** — dark/light theme, Mermaid diagrams, syntax highlighting, WCAG AA accessibility
- **CI/CD ready** — reusable GitHub Actions workflow deploys to GitHub Pages on tag push

## Quick Links

- [Installation](install.md) — Prerequisites and setup
- [Quickstart](quickstart.md) — Init, build, and view in 5 minutes
- [Configuration](configuration.md) — How DocTools reads your package metadata
- [Commands](commands.md) — Full CLI reference
- [Versioning](versioning.md) — Multi-version workflow
- [Translation](translation.md) — Multilingual documentation
- [Template](template.md) — Theme features and customization
- [CI/CD Workflow](workflow.md) — Automated deployment
- [Troubleshooting](troubleshooting.md) — Common issues and fixes

## Requirements

| Requirement | Notes |
|-------------|-------|
| **Bash** | Git Bash on Windows, or native on macOS/Linux |
| **DocFX** | Install with `dotnet tool install -g docfx` |
| **.NET 9+** | Required by DocFX for C# metadata extraction |

## Package Info

| | |
|---|---|
| **Package name** | `com.wayngames.doctools` |
| **Version** | 0.3.0 |
| **Author** | [WAYN Games](https://wayn.games) |

## Next Step

Start with [Installation](install.md) to set up DocTools, then follow the [Quickstart](quickstart.md) to build your first site.
