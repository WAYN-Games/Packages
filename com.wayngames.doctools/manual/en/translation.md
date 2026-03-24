# Translation

DocTools supports translating manual documentation into multiple languages. English is always the source language. Translations are stored alongside the English source and built into separate URL paths with a language switcher for readers.

## How It Works

1. You write documentation in English
2. You add target languages to your package
3. An external tool (such as Claude Code) reads each English file, translates it, and writes it to a locale directory
4. DocTools tracks which files have been translated via SHA-256 hashes
5. The build produces a separate site for each language under `v{version}/{lang}/`
6. A language switcher dropdown appears when more than one language is built

The translation system handles bookkeeping only — it tracks which files need translation and which are up-to-date. The actual translation is performed externally.

## Supported Languages

| Code | Language | Code | Language |
|------|----------|------|----------|
| `fr` | Français | `ja` | 日本語 |
| `de` | Deutsch | `ko` | 한국어 |
| `es` | Español | `zh` | 中文 |
| `it` | Italiano | `ru` | Русский |
| `pt` | Português | `ar` | العربية |
| `nl` | Nederlands | `pl` | Polski |
| `tr` | Türkçe | `sv` | Svenska |

## Adding a Language

Add a language to your package:

```bash
doctools.sh add-language ./MyPackage fr
```

This creates or updates `Documentation/doctools.languages.json`:

```json
["fr"]
```

You can add multiple languages:

```bash
doctools.sh add-language ./MyPackage zh
doctools.sh add-language ./MyPackage ja
```

## Translation Workflow

### Step 1: Check What Needs Translating

```bash
doctools.sh translate ./MyPackage
```

This lists every `.md` file that is missing or outdated for each configured language, showing source and target paths:

```
=== Checking translations for: fr (Français) ===
  Files needing translation:
    - getting-started.md
      Source: /path/to/Documentation/getting-started.md
      Target: /path/to/Documentation/.doctools/locales/fr/getting-started.md
```

### Step 2: Translate Each File

For each pending file, read the English source, translate the content, and write the translated file to the target path.

**Translation rules:**

- Translate all prose, headings, list items, table cell text, and alt text
- Do **not** translate: code blocks, inline code, file paths, class/method names, YAML front matter keys, link URLs, image paths
- Preserve all Markdown formatting, link structure, and document structure exactly
- Keep technical terms that are proper nouns (e.g., "Unity", "DocFX", "Mermaid") untranslated

### Step 3: Mark Each File as Done

After writing a translated file, update the hash manifest:

```bash
doctools.sh translate-done ./MyPackage fr getting-started.md
```

This records the current SHA-256 hash of the English source file. The file will not appear as pending again until the English source changes.

### Step 4: Build

Run the build to include all translations:

```bash
doctools.sh build ./MyPackage
```

Languages with translated files are built into subdirectories (e.g., `v1.0.0/fr/`). Languages with no translated files are skipped with a warning.

## File Structure

Translated files are stored in `.doctools/locales/{lang}/`, mirroring the English file structure:

```
Documentation/
├── getting-started.md                              # English source
├── architecture.md
├── .doctools/
│   └── locales/
│       ├── fr/
│       │   ├── .translation-hashes.json            # Hash manifest
│       │   ├── getting-started.md                  # French translation
│       │   └── architecture.md
│       └── zh/
│           ├── .translation-hashes.json
│           ├── getting-started.md                  # Chinese translation
│           └── architecture.md
```

## Hash-Based Change Tracking

The `.translation-hashes.json` file in each language directory stores the SHA-256 hash of the English source file at the time of translation:

```json
{
  "getting-started.md": "a1b2c3d4...",
  "architecture.md": "e5f6g7h8..."
}
```

When you run `doctools.sh translate`, it compares the current hash of each English file against the stored hash. If they differ, the file appears as needing re-translation. This means:

- New files always appear as pending (no stored hash)
- Edited English files appear as pending (hash changed)
- Unchanged files are skipped (hash matches)

## How Translated Builds Work

For each language, the build system:

1. Copies all English content into a staging directory
2. Overlays translated files on top (replacing the English originals)
3. Builds a separate DocFX site at `v{version}/{lang}/`
4. Generates `languages.js` listing all built languages

This means untranslated files fall back to English automatically — you do not need to translate every file before building.

## Language Switcher

The template includes a language dropdown in the navbar. It appears automatically when more than one language is built. It reads `languages.js` and rewrites URLs to switch between language paths:

- English: `/v1.0.0/manual/getting-started.html`
- French: `/v1.0.0/fr/manual/getting-started.html`
- Chinese: `/v1.0.0/zh/manual/getting-started.html`

## API Docs

API documentation (generated from C# XML comments) is **not translated**. The same English API docs are shared across all language builds.

## Next Steps

- [Commands](commands.md) — CLI reference for translation commands
- [Configuration](configuration.md) — Language configuration
- [Best Practices](best-practices.md) — Writing docs that translate well
