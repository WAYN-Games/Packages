# Configuration

DocTools uses a **convention-over-configuration** approach. Most settings are derived automatically from your `package.json`. No configuration file is required for standard Unity packages.

## How Configuration Works

When you run any DocTools command, it loads settings in this order:

1. **`package.json`** — Package name, display name, version, and author are read from the standard Unity package manifest
2. **Conventions** — Output directory, metadata source, logo, and footer are derived from sensible defaults
3. **`doctools.languages.json`** (optional) — Lists which languages to build translations for

## Required: package.json

Your `package.json` must contain these fields:

```json
{
  "name": "com.company.mypackage",
  "displayName": "My Package",
  "version": "1.0.0",
  "author": {
    "name": "Company Name"
  }
}
```

| Field | Used For |
|-------|----------|
| `name` | Package identifier, URL path segment |
| `displayName` | Navbar title, page titles |
| `version` | Current version number, output directory name |
| `author.name` | Footer copyright text |

## Derived Settings

DocTools automatically computes these values. You do not need to set them.

| Setting | Convention | Description |
|---------|-----------|-------------|
| Output directory | `../Documentation~` | Relative to `Documentation/`. The `~` suffix hides it from Unity's Asset Database |
| Metadata source | `..` | C# source files are scanned from the package root (parent of `Documentation/`) |
| Logo | GitHub org avatar | Uses `https://avatars.githubusercontent.com/u/{org-id}` |
| Favicon | Same as logo | |
| Footer | `© {year} {author.name}` | Auto-generated from package.json author |
| Extra content | `LICENSE.md`, `CHANGELOG.md` | Included in the navbar if present in the package root |
| Resource patterns | `**/*.png` | Images included in the build |
| Namespace layout | `nested` | API docs use nested namespace hierarchy |
| Allow compilation errors | `true` | DocFX metadata succeeds even if C# has missing references |

## Layout Detection

DocTools supports two documentation layouts:

### New Layout (recommended)

Markdown files live directly in `Documentation/`. The root TOC is auto-generated at build time. This is the default for new projects.

```
MyPackage/
├── package.json
└── Documentation/
    ├── toc.md              # Sidebar navigation
    ├── index.md            # Landing page
    ├── install.md          # Your pages...
    └── Images/
```

### Legacy Layout

Markdown files live in `Documentation/Manual/` with a separate `doctools.config` file. DocTools detects this layout automatically if a `Manual/` directory or `doctools.config` exists.

```
MyPackage/
├── package.json
└── Documentation/
    ├── doctools.config     # Legacy config file
    ├── toc.md              # Root navigation
    ├── index.md
    └── Manual/
        ├── toc.md          # Sidebar navigation
        └── *.md
```

> [!NOTE]
> The legacy layout is fully supported but not recommended for new projects. Use the new layout unless you have a specific reason to use `Manual/`.

## Language Configuration

To enable multilingual builds, create `Documentation/doctools.languages.json`:

```json
["fr", "zh", "ja"]
```

This tells DocTools to build French, Chinese, and Japanese versions alongside English. See [Translation](translation.md) for the full workflow.

If this file does not exist, only English is built and no language switcher appears.

## Version Detection

DocTools automatically detects versions:

- **Current version**: read from `package.json`'s `version` field
- **Previous versions**: discovered from existing `v*/` directories in the output folder

No version list needs to be maintained manually. When you bump the version in `package.json` and rebuild, the new version appears alongside the old ones.

## Legacy doctools.config

Older projects may use a `doctools.config` Bash file instead of relying on conventions. If this file exists, DocTools sources it for backward compatibility. See the [legacy config reference](#legacy-config-reference) below.

### Legacy Config Reference

| Field | Type | Description |
|-------|------|-------------|
| `PACKAGE_NAME` | string | Package identifier |
| `PACKAGE_TITLE` | string | Display title |
| `CURRENT_VERSION` | string | Current version |
| `ALL_VERSIONS` | array | All versions to build |
| `PACKAGE_FOOTER` | string | Footer HTML |
| `METADATA_SRC` | string | C# source path |
| `LOGO_PATH` | string | Logo image path |
| `OUTPUT_DIR` | string | Build output directory |
| `EXTRA_CONTENT` | array | Additional files to include |
| `RESOURCE_PATTERNS` | array | Image glob patterns |

> [!TIP]
> To migrate from legacy config to conventions: delete `doctools.config`, ensure your `package.json` has the required fields, and move docs out of `Manual/` into `Documentation/` directly.

## Next Steps

- [Commands](commands.md) — Full CLI reference
- [Versioning](versioning.md) — Multi-version workflow
- [Translation](translation.md) — Multilingual support
