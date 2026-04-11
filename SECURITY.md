# Security policy

## Reporting a vulnerability

Thank you for helping keep WAYN Games packages secure.

**Do not file a public issue for security vulnerabilities.** Public reports give attackers a head start before a fix can be released.

### How to report

Use GitHub's **[Private vulnerability reporting](https://github.com/WAYN-Games/docs.wayn.games/security/advisories/new)** on this repository. The report is private — only repository maintainers can see it.

Include:

- The affected package and version
- A clear description of the vulnerability
- Steps to reproduce, or a proof-of-concept
- The impact (what an attacker could do, what data is at risk)
- Any mitigations or workarounds you're aware of

### What to expect

- **Acknowledgement** within 5 business days
- **Initial assessment** within 10 business days, including a severity rating and an indication of whether a fix is planned
- **Fix and disclosure** depending on severity:
  - **Critical** — patch release as soon as possible, with a coordinated disclosure
  - **High** — patch in the next scheduled release
  - **Medium / low** — bundled into a future release at the maintainers' discretion

You will be credited in the release notes for the fix unless you ask to remain anonymous.

### Out of scope

- Vulnerabilities in **your own code** that uses WAYN Games packages
- Vulnerabilities in **Unity itself** or in third-party dependencies — please report those upstream
- Theoretical issues without a working proof-of-concept
- Issues that require physical access to a victim's machine

### Sensitive information

The same rules from [`SUPPORT.md`](SUPPORT.md) apply, with one exception: in a **private** vulnerability report, you may include WAYN Games package source code if it is necessary to demonstrate the issue. Do not paste it into the public issue tracker under any circumstances.
