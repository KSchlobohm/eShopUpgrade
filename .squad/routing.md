# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Migration strategy, architecture | Keaton | Migration order, adapter strategy, dependency decisions |
| csproj conversion, code migration | McManus | SDK-style conversion, namespace changes, EF6→EF Core |
| Research, dependency analysis | Fenster | Find upgrade guides, analyze breaking changes, compatibility research |
| Build verification, testing | Hockney | Compile checks, startup tests, regression tests |
| Code review | Keaton | Review PRs, approve migration steps |
| MSMQ replacement | McManus | Experimental.System.Messaging, namespace changes |
| System.Web adapter integration | McManus | Microsoft.AspNetCore.SystemWebAdapters setup |
| EF6 to EF Core migration | McManus | DbContext, migrations, query patterns |
| Autofac to Core DI | McManus | DI container conversion |
| Identity migration | McManus | ASP.NET Identity v2 → Core Identity |
| Test framework migration | Hockney | MSTest updates, test project conversion |
| Scope & priorities | Keaton | What to migrate next, trade-offs, decisions |
| Async issue work (bugs, tests, small features) | @copilot 🤖 | Well-defined tasks matching capability profile |
| Session logging | Scribe | Automatic — never needs routing |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, evaluate @copilot fit, assign `squad:{member}` label | Keaton |
| `squad:keaton` | Architecture decisions, migration planning | Keaton |
| `squad:mcmanus` | Code migration, csproj conversion | McManus |
| `squad:fenster` | Research, dependency analysis | Fenster |
| `squad:hockney` | Testing, build verification | Hockney |
| `squad:copilot` | Assign to @copilot for autonomous work (if enabled) | @copilot 🤖 |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Keaton** triages it — analyzing content, evaluating @copilot's capability profile, assigning the right `squad:{member}` label, and commenting with triage notes.
2. **@copilot evaluation:** Keaton checks if the issue matches @copilot's capability profile (🟢 good fit / 🟡 needs review / 🔴 not suitable). If it's a good fit, Keaton may route to `squad:copilot` instead of a squad member.
3. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
4. When `squad:copilot` is applied and auto-assign is enabled, `@copilot` is assigned on the issue and picks it up autonomously.
5. Members can reassign by removing their label and adding another member's label.
6. The `squad` label is the "inbox" — untriaged issues waiting for Keaton's review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what branch are we on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If McManus is migrating a project, spawn Hockney to write verification checks simultaneously.
7. **Fenster first for unknowns.** If the migration path is unclear, spawn Fenster to research before McManus implements.
8. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Keaton handles all `squad` (base label) triage.
9. **@copilot routing** — when evaluating issues, check @copilot's capability profile in `team.md`. Route 🟢 good-fit tasks to `squad:copilot`. Flag 🟡 needs-review tasks for PR review. Keep 🔴 not-suitable tasks with squad members.
