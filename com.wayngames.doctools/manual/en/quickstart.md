# Quickstart

This guide gets you from zero to a built documentation site in under 5 minutes.

**Goal**: Initialize documentation for a Unity package and view the generated site in your browser.

## Prerequisites

- DocTools installed ([Installation](install.md))
- A Unity package with a `package.json` containing `name`, `displayName`, and `version` fields

## Step 1: Initialize Documentation

Run `init` pointing at your package directory:

```bash
doctools.sh init ./MyPackage
```

This creates a `Documentation/` directory with starter files:

```
MyPackage/
├── package.json
└── Documentation/
    ├── toc.md              # Sidebar table of contents
    ├── index.md            # Landing page
    ├── getting-started.md  # Starter guide
    └── Images/
        └── logo.png        # Default logo
```

The title is read automatically from `package.json`'s `displayName` field. To override it:

```bash
doctools.sh init ./MyPackage --title "My Custom Title"
```

## Step 2: Add a Page

Open `Documentation/getting-started.md` in your editor and add some content. The scaffolded file already has a basic structure you can customize.

To add a new page:

1. Create a new `.md` file in `Documentation/` (e.g., `architecture.md`)
2. Add a link to it in `Documentation/toc.md`:
   ```markdown
   # [Architecture](architecture.md)
   ```

## Step 3: Build

Run the build command:

```bash
doctools.sh build ./MyPackage
```

Output goes to `MyPackage/Documentation~/`:

```
MyPackage/
└── Documentation~/
    ├── index.html      # Redirect to current version
    ├── versions.js     # Version switcher data
    └── v1.0.0/
        ├── index.html  # Landing page
        ├── manual/     # Your markdown pages (HTML)
        ├── api/        # C# API reference (auto-generated)
        └── styles/     # Theme CSS and JavaScript
```

## Step 4: View the Site

Open the generated site in your browser:

```bash
# Windows (Git Bash)
start MyPackage/Documentation~/v1.0.0/index.html

# macOS
open MyPackage/Documentation~/v1.0.0/index.html

# Linux
xdg-open MyPackage/Documentation~/v1.0.0/index.html
```

You should see a documentation site with:

- A **navbar** with your package title, search box, and dark mode toggle
- A **sidebar** with your table of contents
- Your **content** rendered from markdown
- An **API** section with auto-generated C# reference docs

## What Just Happened?

The `build` command ran a pipeline:

1. **Metadata extraction** — DocFX scanned your C# source files and generated API documentation (YAML files)
2. **Content processing** — DocFX converted your markdown pages and API YAML into HTML
3. **Template application** — The WaynGames.StaticToc.Extension template added dark/light mode, version switcher, search, and styling
4. **Output generation** — `versions.js` and a root redirect were created for the version switcher

The version number (`v1.0.0`) came from your `package.json`'s `version` field. When you bump the version and rebuild, both versions appear in the version dropdown.

## Next Steps

- [Configuration](configuration.md) — Understand how DocTools reads your package metadata
- [Versioning](versioning.md) — Build documentation for multiple versions
- [Translation](translation.md) — Add multilingual support
- [Commands](commands.md) — Full CLI reference
