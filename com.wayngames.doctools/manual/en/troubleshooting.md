# Troubleshooting

Common issues organized by symptom.

## Build fails with "command not found: docfx"

**Cause**: DocFX is not installed or not on your PATH.

**Fix**:
1. Install DocFX: `dotnet tool install -g docfx`
2. Verify: `docfx --version`
3. If installed but not found, add the .NET tools directory to your PATH (typically `~/.dotnet/tools`)

## Build shows C# compilation warnings

**Cause**: DocFX scans your C# source files and reports compilation errors when Unity-specific assemblies (UnityEngine, UnityEditor) cannot be resolved outside the Unity Editor.

**Fix**: These warnings are expected and do not affect the build. API docs are still generated. The `METADATA_ALLOW_COMPILATION_ERRORS` convention is set to `true` by default to handle this.

## Version switcher does not appear

**Cause**: There is only one version in the output directory, or `versions.js` was not generated.

**Fix**:
1. Verify that `Documentation~/versions.js` exists after building
2. Build with at least two versions present (bump `version` in `package.json` and rebuild)
3. If viewing locally via `file://`, some browsers block JavaScript loading — use a local HTTP server instead

## Language switcher does not appear

**Cause**: No `doctools.languages.json` exists, or no translations have been built.

**Fix**:
1. Add a language: `doctools.sh add-language ./MyPackage fr`
2. Translate at least one file and mark it done
3. Rebuild — the language switcher appears when more than one language is built
4. Verify `Documentation~/languages.js` exists and lists multiple languages

## Translated pages have wrong layout

**Cause**: The translated `toc.md` is being used as the root navbar TOC instead of the sidebar TOC.

**Fix**: The translated `toc.md` should only contain sidebar navigation (the manual page list). The root navbar TOC (Manual, API, LICENSE, CHANGELOG) is always generated from English. If you see the full page list in the navbar, check that your locale's `toc.md` is structured as a sidebar TOC, not a root TOC.

## Links to LICENSE/CHANGELOG are broken in translated pages

**Cause**: Relative paths in the root TOC do not account for the language subdirectory nesting.

**Fix**: Update DocTools to the latest version. This was fixed in the language-aware root TOC generation.

## Mermaid diagrams do not render

**Cause**: The code block is not tagged with the correct language.

**Fix**: Use `` ```mermaid `` (not `` ```mmd `` or `` ```diagram ``). The Mermaid renderer looks for the `lang-mermaid` CSS class, which DocFX generates from the fenced code block language tag.

## Search does not work

**Cause**: The search index (`index.json`) was not generated, or JavaScript is blocked.

**Fix**:
1. Verify `index.json` exists in the version output directory
2. If viewing via `file://`, switch to a local HTTP server — browsers restrict `fetch()` and `XMLHttpRequest` on `file://` origins
3. For cross-package search, verify `packages.json` exists at the site root

## Build succeeds but site looks unstyled

**Cause**: The template path is not being resolved correctly.

**Fix**:
1. Verify the DocTools repository is intact (the `templates/` directory exists)
2. Check that `styles/main.css` exists in the build output (`Documentation~/v{version}/styles/`)
3. If the template directory was moved, ensure the path in `docfx-gen.sh` resolves correctly

## "No languages configured" when running translate

**Cause**: `Documentation/doctools.languages.json` does not exist.

**Fix**: Add a language first:
```bash
doctools.sh add-language ./MyPackage fr
```

## Translation shows files as pending after translate-done

**Cause**: The English source file was modified after running `translate-done`, or the wrong relative path was used.

**Fix**:
1. Verify the relative path matches exactly (e.g., `getting-started.md`, not `./getting-started.md`)
2. Check that the English source has not been modified since running `translate-done`
3. Re-run `translate-done` if the English source was updated

## Next Steps

- [Commands](commands.md) — CLI reference
- [Configuration](configuration.md) — How settings work
- [Template](template.md) — Template features
