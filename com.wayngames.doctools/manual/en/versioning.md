# Versioning

DocTools builds documentation for multiple versions of a package simultaneously. Each version gets its own output directory, and the site includes a dropdown to switch between them.

## How Versions Work

- The **current version** is read from `package.json`'s `version` field
- **Previous versions** are discovered automatically from existing `v*/` directories in the output folder (`Documentation~/`)
- Each build produces output in `Documentation~/v{version}/`
- A `versions.js` file lists all available versions for the dropdown
- A root `index.html` redirects to the current version

No version list needs to be maintained manually.

## Typical Workflow

### 1. Start with Your First Version

After `doctools.sh init`, write your documentation and build:

```bash
doctools.sh build ./MyPackage
```

This creates `Documentation~/v1.0.0/` (assuming `version` is `1.0.0` in `package.json`).

### 2. Bump the Version

When releasing a new version:

1. Update `version` in `package.json` (e.g., `1.0.0` → `1.1.0`)
2. Update your documentation as needed
3. Update `CHANGELOG.md`

### 3. Build Again

```bash
doctools.sh build ./MyPackage
```

The build produces `Documentation~/v1.1.0/` alongside the existing `v1.0.0/`. Both versions appear in the version dropdown.

### 4. Deploy

Push a git tag to trigger CI/CD deployment:

```bash
git tag v1.1.0
git push --tags
```

See [CI/CD Workflow](workflow.md) for the automated deployment pipeline.

## Version Switcher

The built site includes a version dropdown in the breadcrumb bar. It is populated at runtime from `versions.js`:

```javascript
window.DOC_VERSIONS = [{"version":"1.1.0"},{"version":"1.0.0"}];
```

When a user selects a different version, they are navigated to that version's landing page. If the user is viewing a translated page, the switcher attempts to navigate to the same language in the new version, falling back to English if that language is not available.

## Removing Old Versions

To remove a previously built version:

```bash
doctools.sh version ./MyPackage remove 1.0.0
```

This deletes the `Documentation~/v1.0.0/` directory. You cannot remove the current version.

To list all versions:

```bash
doctools.sh version ./MyPackage list
```

## Output Structure

After several versions, the output looks like:

```
Documentation~/
├── index.html          # Redirect to v1.1.0
├── versions.js         # [{"version":"1.1.0"},{"version":"1.0.0"}]
├── languages.js        # Language switcher data (if multilingual)
├── v1.1.0/             # Current version
│   ├── index.html
│   ├── manual/
│   ├── api/
│   └── styles/
└── v1.0.0/             # Previous version
    ├── index.html
    ├── manual/
    ├── api/
    └── styles/
```

## Next Steps

- [Translation](translation.md) — Add multilingual support
- [CI/CD Workflow](workflow.md) — Automated deployment on tag push
- [Commands](commands.md) — Full CLI reference
