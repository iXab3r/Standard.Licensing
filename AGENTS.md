# Repository guidance

This checkout is the iXab3r fork of Standard.Licensing. [docs/README.md](docs/README.md) preserves upstream usage, attribution, and package links; those links do not establish that this fork has published the same artifacts. The actual target frameworks, package dependencies, signing, and package inputs are defined in [the library project](src/Standard.Licensing/Standard.Licensing.csproj), not by broad platform names in inherited prose.

Source and the solution live under `src/`; [Standard.Licensing.Tests](src/Standard.Licensing.Tests/Standard.Licensing.Tests.csproj) is the NUnit test project. Follow the current fork's API, including product identity, rather than restoring an old upstream signature from a usage example. Preserve license serialization, signature/key formats, product identity semantics, and signing identity unless the task explicitly changes that contract. Old persisted licenses and public-key consumers are real compatibility boundaries.

Keep upstream credits, [LICENSE](LICENSE), and [ACKNOWLEDGEMENTS.md](ACKNOWLEDGEMENTS.md). `external/bc-csharp` is a declared submodule; do not initialize or replace it merely because the directory is empty. Inspect actual project references before selecting build prerequisites.

For code changes, build the checked-in solution and run the focused NUnit project; distinguish compilation, behavioral tests, and cross-framework evidence. Documentation-only maintenance requires factual, local-link, and diff checks with unrun code checks disclosed. Do not publish packages or generate/disclose production signing keys as documentation validation.

## Independent architecture and review

Start each delegated architect, advisor, and reviewer with empty conversation context (`fork_turns="none"` with `spawn_agent`, or the host's equivalent). Provide only the concrete question, repository path and relevant revisions, scope, constraints, and raw evidence locations. Keep the brief focused on the actual topic; omit the author's reasoning, preferred verdict, and earlier agents' conclusions.

Each agent independently inspects the relevant code, contracts, and tests and reports its own evidence, conclusions, and uncertainty. The final reviewer must be separate from the implementer and design advisor, remain read-only, and inspect the final diff at the named revision. Disclose unavailable independent review rather than presenting self-review as independent.
