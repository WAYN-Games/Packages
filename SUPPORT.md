# Getting help with WAYN Games packages

This repository is the **public issue tracker for every package developed by WAYN Games**. Whether you found a bug, want to request a feature, or spotted a documentation problem, this is the right place to file it.

## Open an issue

Pick one of the templates from the [New issue page](https://github.com/WAYN-Games/docs.wayn.games/issues/new/choose):

- **[Bug report](https://github.com/WAYN-Games/docs.wayn.games/issues/new?template=bug-report.yml)** — something doesn't work as documented
- **[Feature request](https://github.com/WAYN-Games/docs.wayn.games/issues/new?template=feature-request.yml)** — suggest a new feature or improvement
- **[Documentation issue](https://github.com/WAYN-Games/docs.wayn.games/issues/new?template=documentation-issue.yml)** — error, gap, or improvement in the docs

Each template asks which package the report is about. Please fill in every required field — incomplete reports take longer to triage.

## Before you file

- **Search [existing issues](https://github.com/WAYN-Games/docs.wayn.games/issues?q=is%3Aissue)** to avoid duplicates
- Make sure you're on a **supported version** of the package — older versions may have known fixes already shipped
- For bug reports, prepare a **minimal reproduction** if possible — a small public Unity project that triggers the bug helps the most

## What to expect

WAYN Games packages are developed in **private repositories**. When you file an issue here:

1. It will be triaged and labeled with the relevant package, type, and status
2. Once acknowledged, the work is tracked internally and you'll see a `status: acknowledged` label
3. When a fix or feature ships, the issue is closed with a reference to the release that contains it

You will **not** see internal commits, pull requests, or private discussion threads — those live in the package's private repository. Public communication about your issue happens entirely on the issue itself.

## Sensitive information

This repository is public. **Do not include**:

- Credentials, API keys, license keys, or tokens
- Proprietary code from your project
- Personal data
- Anything you wouldn't want indexed by search engines
- **WAYN Games package source code.** The packages are commercial products and their internal code must not be redistributed in public issues. To reproduce a bug, share your own code that *uses* the package (plus stack traces from the Unity console) — never the package's own source files. Quoting short snippets from the public documentation when reporting a docs issue is fine; pasting full source files from `Runtime/`, `Editor/`, or `Tests/` is not.

If you need to share something sensitive to reproduce a bug, mention it in the issue and a maintainer will reach out via email.
