---
uid: workflow
---

# Package Lifecycle & Documentation Workflow

This page describes the end-to-end workflow for managing packages with versioned documentation, from initial setup through automated deployment.

## Package Lifecycle

### First-Time Setup

These steps are done once per package.

#### 1. Create the package

Create a Unity package with a `package.json` containing at minimum:

```json
{
  "name": "com.wayngames.mypackage",
  "displayName": "My Package",
  "version": "1.0.0",
  "author": { "name": "WAYN Games" }
}
```

#### 2. Initialize documentation

```bash
doctools.sh init ./MyPackage
```

This creates `Documentation/` with a table of contents, index page, and starter guide.

#### 3. Add languages (optional)

If you want multilingual documentation, add each target language:

```bash
doctools.sh add-language ./MyPackage fr
doctools.sh add-language ./MyPackage zh
```

#### 4. Add the CI/CD workflow

Add `.github/workflows/docs.yml` to the package repo (see [CI/CD setup](#adding-a-new-package) below).

#### 5. Initial commit and release

```bash
git add Documentation/ .github/
git commit -m "docs: initialize documentation"
git tag v1.0.0
git push origin main --tags
```

---

### Feature Development Workflow

This is the day-to-day workflow for adding features with documentation.

#### 1. Create a feature branch

```bash
git checkout main
git pull origin main
git checkout -b feature/my-feature
```

#### 2. Develop

Write code, add or update documentation pages in `Documentation/`, and commit as you go:

```bash
# ... edit files ...
git add src/MyNewFeature.cs Documentation/my-new-feature.md Documentation/toc.md
git commit -m "feat: add my new feature"
```

Repeat as needed — multiple commits on the feature branch are fine.

#### 3. Merge to main

When the feature is complete:

```bash
git checkout main
git pull origin main
git merge --no-ff feature/my-feature
git branch -d feature/my-feature
```

---

### Release Workflow

When you are ready to release a new version with updated documentation.

#### 1. Bump the version

Edit `package.json` to update the `version` field (e.g., `1.0.0` → `1.1.0`). Update `CHANGELOG.md` with the changes.

```bash
# Edit package.json and CHANGELOG.md, then:
git add package.json CHANGELOG.md
git commit -m "chore: bump version to 1.1.0"
```

#### 2. Translate documentation (if multilingual)

Check which files need translation:

```bash
doctools.sh translate ./MyPackage
```

For each file listed as pending, translate the English source and write the translated file to the target path shown in the output. Then mark each file as done:

```bash
# After writing the translated file to .doctools/locales/fr/my-page.md:
doctools.sh translate-done ./MyPackage fr my-page.md

# Repeat for each file and language
doctools.sh translate-done ./MyPackage fr another-page.md
doctools.sh translate-done ./MyPackage zh my-page.md
doctools.sh translate-done ./MyPackage zh another-page.md
```

Commit the translations:

```bash
git add Documentation/.doctools/locales/
git commit -m "docs: update translations for v1.1.0"
```

#### 3. Build and verify locally

```bash
doctools.sh build ./MyPackage
```

Open the output to verify:

```bash
# Windows (Git Bash)
start MyPackage/Documentation~/v1.1.0/index.html

# macOS
open MyPackage/Documentation~/v1.1.0/index.html
```

Check that:
- English pages render correctly
- Translated pages render with correct layout
- Version switcher lists all versions
- Language switcher appears and switches correctly

#### 4. Tag and push

```bash
git tag v1.1.0
git push origin main --tags
```

The tag push triggers the GitHub Actions workflow, which:
1. Checks out the package repo and DocTools
2. Fetches previously published versions from `gh-pages`
3. Runs `doctools.sh build`
4. Deploys to GitHub Pages

The live site includes the version switcher, language switcher, and all translations.

## Documentation Workflow (CI/CD)

When a version tag (`v*`) is pushed, the following automated flow executes:

```text
Package Repo                    DocTools                        Packages (gh-pages)
    |                               |                               |
    |-- tag v1.0.0 pushed --------->|                               |
    |   docs.yml triggers           |                               |
    |   calls deploy-docs.yml ----->|                               |
    |                               |-- checkout package repo       |
    |                               |-- checkout DocTools            |
    |                               |-- install .NET + DocFX        |
    |                               |-- fetch existing v*/ -------->|
    |                               |   from gh-pages               |
    |                               |-- doctools.sh build           |
    |                               |-- deploy Documentation~/ --->|
    |                               |   to /{package.name}/         |
    |                               |                               |-- GitHub Pages serves
    |                               |                               |   docs.wayn.games/{name}/
```

### Step by step

1. A `v*` tag push triggers the package repo's `.github/workflows/docs.yml`
2. `docs.yml` calls the reusable workflow at `WAYN-Games/DocTools/.github/workflows/deploy-docs.yml`
3. The reusable workflow:
   - Checks out the package repo and DocTools (private repo, authenticated via PAT)
   - Installs .NET SDK and DocFX
   - Sparse-checkouts previously published version directories from the `gh-pages` branch of `WAYN-Games/Packages` into `Documentation~/` so the version switcher includes all historical versions
   - Runs `doctools.sh build` which generates the DocFX site, `versions.js`, and a root `index.html` redirect
   - Deploys the full `Documentation~/` output to `WAYN-Games/Packages` gh-pages under `/{package.name}/` using `keep_files: true` (preserving other packages' docs)
4. GitHub Pages serves the content at `docs.wayn.games/{package.name}/`
5. The version-switcher dropdown (populated from `versions.js`) lets users navigate between versions

## Repository Roles

| Repository | Visibility | Role |
|---|---|---|
| `WAYN-Games/DocTools` | Private | Build CLI + reusable GitHub Actions workflow |
| `WAYN-Games/Packages` | Public | Hosts `gh-pages` branch, custom domain `docs.wayn.games` |
| Package repos | Private or Public | Source code + doc source, each has a `docs.yml` caller workflow |

## URL Structure

All documentation is served from a single domain:

```text
docs.wayn.games/
  index.html                              <- landing page listing all packages
  com.wayngames.doctools/
    index.html                            <- redirect to latest version
    versions.js                           <- version manifest for dropdown
    v0.1.0/                               <- DocFX output for v0.1.0
    v0.2.0/                               <- DocFX output for v0.2.0
  com.wayngamesassets.mgm.charactercontroller/
    index.html
    versions.js
    v0.3.0/
```

The URL path segment is the `name` field from the package's `package.json`.

## Adding a New Package

1. Initialize documentation: `doctools.sh init ./NewPackage`
2. Write docs and build locally: `doctools.sh build ./NewPackage`
3. Add `.github/workflows/docs.yml` to the package repo:

    ```yaml
    name: Documentation

    on:
      push:
        tags: ['v*']
      workflow_dispatch:

    jobs:
      docs:
        uses: WAYN-Games/DocTools/.github/workflows/deploy-docs.yml@main
        with:
          package-name: com.wayngames.newpackage
        secrets:
          doctools-token: ${{ secrets.DOCTOOLS_READ_TOKEN }}
          deploy-token: ${{ secrets.PACKAGES_DEPLOY_TOKEN }}
    ```

4. Add `DOCTOOLS_READ_TOKEN` and `PACKAGES_DEPLOY_TOKEN` secrets to the repo (or use org-level secrets)
5. Tag a release (`git tag v1.0.0 && git push --tags`) to trigger deployment
6. Add an entry to `landing/index.html` in the Packages repo

## Removing a Version

To remove a published version:

1. Locally: `doctools.sh version ./PackageDir remove X.Y.Z`
2. Re-trigger the docs workflow (via `workflow_dispatch`) to update `versions.js` and the root redirect on the deployed site

## Authentication Setup

### Reusable Workflow Access

Since DocTools is a private repository, its reusable workflows must be explicitly shared with the organization:

1. Go to **https://github.com/WAYN-Games/DocTools/settings/actions**
2. Under **Access**, select **"Accessible from repositories in the 'WAYN-Games' organization"**

This is a one-time setup. Without it, other repos cannot call the reusable workflow.

### Personal Access Tokens

The system uses two fine-grained Personal Access Tokens for least-privilege access:

| Secret Name | Scope | Permission | Purpose |
|---|---|---|---|
| `DOCTOOLS_READ_TOKEN` | `WAYN-Games/DocTools` | Contents: Read-only | Checkout the private build tooling |
| `PACKAGES_DEPLOY_TOKEN` | `WAYN-Games/Packages` | Contents: Read and write | Push built docs to `gh-pages` |

Both secrets must be added to each package repo (or stored as organization-level secrets for convenience).
