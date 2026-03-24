# Best Practices

Recommendations for getting the most out of DocTools.

## Project Structure

### Use the new layout

Place markdown files directly in `Documentation/` rather than in a `Manual/` subdirectory. The new layout auto-generates the root TOC and requires no `doctools.config` file.

### Keep Documentation/ focused

Only put documentation-related files in `Documentation/`:
- Markdown pages (`.md`)
- Images (`Images/` subdirectory)
- Translation files (`.doctools/locales/`)

Build output goes to `Documentation~/` (auto-excluded from Unity's Asset Database).

### Organize with subdirectories

For packages with many pages, use subdirectories to group related content:

```
Documentation/
├── toc.md
├── index.md
├── install.md
├── quickstart.md
├── recipes/
│   ├── add-a-feature.md
│   └── integrate-with-x.md
├── integrations/
│   ├── input-system.md
│   └── cinemachine.md
└── Images/
```

Update `toc.md` to reflect the hierarchy:

```markdown
# [Installation](install.md)
# [Quickstart](quickstart.md)
# Recipes
## [Add a Feature](recipes/add-a-feature.md)
## [Integrate with X](recipes/integrate-with-x.md)
```

## Writing for Translation

If your documentation will be translated, follow these guidelines to produce cleaner translations:

### Keep sentences short and direct

Short sentences translate more accurately than long compound sentences.

### Avoid idioms and colloquialisms

Phrases like "out of the box" or "under the hood" do not translate well. Use precise language instead.

### Use consistent terminology

Define key terms once and use them consistently. Do not alternate between synonyms (e.g., "entity" vs "object" vs "item" for the same concept).

### Keep code blocks and technical terms in English

Translators should not translate code, class names, method names, or Unity-specific terms. The translation rules in DocTools enforce this, but writing with this in mind helps.

## Versioning

### Bump version before building

Always update `version` in `package.json` before running `doctools.sh build`. The version number determines the output directory name.

### Keep previous versions

Do not delete old version directories from `Documentation~/` unless necessary. The version switcher lets users access documentation for the version they are using.

### Update CHANGELOG.md

Maintain a `CHANGELOG.md` in your package root. DocTools automatically includes it in the navbar alongside LICENSE.

## Mermaid Diagrams

### Use Mermaid for architecture and flow diagrams

Mermaid diagrams render directly in the documentation with no image files to maintain. They update automatically when you edit the markdown.

### Keep diagrams simple

Complex diagrams with many nodes become hard to read. Break large diagrams into multiple smaller ones, each focused on one aspect.

### Test diagrams locally

Build and view the site to verify diagrams render correctly. Mermaid syntax errors produce blank blocks with no visible error message in the built output.

## Images

### Use relative paths

Reference images with relative paths from the markdown file:

```markdown
![Screenshot](Images/screenshot.png)
```

### Keep images in Documentation/Images/

Store all images in the `Images/` subdirectory. The default resource pattern (`**/*.png`) picks them up automatically.

### Use PNG for screenshots, SVG for diagrams

PNG works well for screenshots and UI captures. For diagrams, prefer Mermaid (rendered at build time) over static image files.

## CI/CD

### Deploy on tag push

Use the reusable GitHub Actions workflow to deploy documentation automatically when you push a version tag. See [CI/CD Workflow](workflow.md).

### Commit translations to the repository

Translated files in `.doctools/locales/` should be committed to version control. The CI build picks them up automatically — no translation happens during CI.

## Next Steps

- [Quickstart](quickstart.md) — Get started with DocTools
- [Translation](translation.md) — Multilingual workflow
- [Template](template.md) — Template features and customization
