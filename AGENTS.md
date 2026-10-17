# Repository Guidelines

## Project Structure & Module Organization

Olivaw Hub is currently a planning repository for technology services aimed at small businesses. Implementation has not started.

- `README.md` introduces the project and links to its initial context.
- `BRIEFING.md` records the business proposal, intended technical direction, and unresolved decisions.

There are no source, test, or asset directories yet. Angular templates, an API template, schematics, and AWS deployment are proposed directions, not implemented components. Document agreed architecture before introducing a module layout or infrastructure.

## Build, Test, and Development Commands

No build system, dependency manifest, development server, or automated test commands exist yet. Do not assume commands such as `npm install`, `npm test`, or `ng serve` are available.

For documentation changes, inspect the rendered Markdown and check that relative links resolve, especially the link from `README.md` to `BRIEFING.md`. When adding executable code, document its installation, local development, build, and test commands alongside the implementation.

## Coding Style & Naming Conventions

Keep existing project documentation in Portuguese and preserve the **Olivaw Hub** brand name. Use concise Markdown headings, short paragraphs, and hyphenated bullet lists. Wrap commands and paths in backticks, and use relative links for repository documents.

No programming-language style, indentation standard, formatter, or linter has been established. Choose and document these when introducing the first implementation; follow the resulting configuration consistently.

## Testing Guidelines

There is no test framework or coverage requirement yet. Review documentation for factual consistency and clearly distinguish confirmed decisions from proposals. For future executable components, introduce relevant tests and document the framework, test naming convention, and execution command in the same change.

## Commit & Pull Request Guidelines

Git history is unavailable in this checkout, so no existing commit convention can be verified. Use short, descriptive, imperative commit subjects, such as `docs: clarify initial service offering`.

Pull requests should explain the change, its purpose, and how it was checked. Link relevant issues when available. Update `BRIEFING.md` when business or technical decisions change; include screenshots for future visible interface changes.

## Security & Configuration

Never commit AWS credentials, access tokens, customer data, or local secrets. When infrastructure is introduced, document required configuration using examples with placeholder values.
