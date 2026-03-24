# Commands

All DocTools commands follow the same pattern:

```bash
doctools.sh <command> <package-dir> [arguments]
```

`<package-dir>` is the path to the Unity package root (the directory containing `package.json`).

---

## init

Scaffold a `Documentation/` directory with starter files.

```bash
doctools.sh init <package-dir> [--title TITLE]
```

| Flag | Description | Default |
|------|-------------|---------|
| `--title TITLE` | Override the documentation title | `displayName` from `package.json` |

### What It Creates

```
Documentation/
├── toc.md              # Sidebar table of contents
├── index.md            # Landing page
├── getting-started.md  # Starter guide
└── Images/
    └── logo.png        # Default logo
```

If a `Documentation/` directory already exists, the command exits without overwriting.

### Example

```bash
doctools.sh init ./MyPackage
doctools.sh init ./MyPackage --title "My Custom Docs"
```

---

## build

Build the documentation site. This is the main command you run during development and in CI.

```bash
doctools.sh build <package-dir>
```

### What It Does

1. Loads configuration from `package.json` and conventions
2. Runs `docfx metadata` to extract C# API docs
3. Runs `docfx build` for the English version
4. For each configured language with translated files, runs `docfx build` for that language
5. Generates `versions.js`, `languages.js`, and a root `index.html` redirect

### Output Structure

```
Documentation~/
├── index.html          # Redirect to current version
├── versions.js         # Version switcher data
├── languages.js        # Language switcher data
└── v1.0.0/
    ├── index.html      # Landing page
    ├── manual/         # Manual pages (HTML)
    ├── api/            # API reference (HTML)
    ├── styles/         # CSS and JavaScript
    ├── fr/             # French translation (if configured)
    │   ├── manual/
    │   └── ...
    └── zh/             # Chinese translation (if configured)
        └── ...
```

### Example

```bash
doctools.sh build ./MyPackage
```

---

## translate

List all documentation files that need translation for each configured language.

```bash
doctools.sh translate <package-dir>
```

For each language in `doctools.languages.json`, this command compares the SHA-256 hash of each English source file against the stored hash from the last translation. Files that are new or have changed since their last translation are listed with their source and target paths.

### Example Output

```
=== Checking translations for: fr (Français) ===
  Files needing translation:
    - getting-started.md
      Source: /path/to/Documentation/getting-started.md
      Target: /path/to/Documentation/.doctools/locales/fr/getting-started.md

=== Checking translations for: zh (中文) ===
  All files up to date.
```

See [Translation](translation.md) for the full workflow.

---

## translate-done

Mark a file as translated by updating its hash in the translation manifest.

```bash
doctools.sh translate-done <package-dir> <lang-code> <relative-path>
```

| Argument | Description |
|----------|-------------|
| `<lang-code>` | Language code (e.g., `fr`, `zh`, `ja`) |
| `<relative-path>` | Path relative to `Documentation/` (e.g., `getting-started.md`) |

Run this after writing the translated file to `.doctools/locales/{lang}/{path}`. It records the current English source hash so the file won't appear as pending until the English source changes.

### Example

```bash
doctools.sh translate-done ./MyPackage fr getting-started.md
doctools.sh translate-done ./MyPackage zh recipes/add-a-new-state.md
```

---

## add-language

Add a language to the package's translation configuration.

```bash
doctools.sh add-language <package-dir> <lang-code>
```

This creates or updates `Documentation/doctools.languages.json`. Run with no language code to see all supported codes.

### Supported Languages

| Code | Language | Code | Language |
|------|----------|------|----------|
| `fr` | Français | `ja` | 日本語 |
| `de` | Deutsch | `ko` | 한국어 |
| `es` | Español | `zh` | 中文 |
| `it` | Italiano | `ru` | Русский |
| `pt` | Português | `ar` | العربية |
| `nl` | Nederlands | `pl` | Polski |
| `tr` | Türkçe | `sv` | Svenska |

### Example

```bash
doctools.sh add-language ./MyPackage fr
doctools.sh add-language ./MyPackage zh
```

---

## version

Manage documentation versions.

### version list

List the current version and all previously built versions.

```bash
doctools.sh version <package-dir> list
```

The current version (from `package.json`) is marked with `*`. Previous versions are discovered from `v*/` directories in the output folder.

### version remove

Remove a previously built version from the output directory.

```bash
doctools.sh version <package-dir> remove <version>
```

You cannot remove the current version.

### Example

```bash
doctools.sh version ./MyPackage list
doctools.sh version ./MyPackage remove 0.1.0
```

---

## Next Steps

- [Configuration](configuration.md) — How settings are derived
- [Versioning](versioning.md) — Multi-version workflow
- [Translation](translation.md) — Multilingual documentation workflow
