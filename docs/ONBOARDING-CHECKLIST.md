# Developer Onboarding Checklist

Use this checklist when bringing a new contributor onto the TextEdit project. Capture completion in your team tracker or onboarding issue.

## Accounts & Access
- [ ] Confirm access to GitHub repository and required organizations.
- [ ] Share CI/CD dashboard links (GitHub Actions, NuGet feeds, telemetry dashboards when available).
- [ ] Provide access to shared storage (design assets, performance logs).

## Local Environment Setup
- [ ] Install .NET SDK 8.0 LTS and preview 10.0 SDK if working with bleeding-edge features.
- [ ] Install Node.js 18+ (required for VS Code-compatible extension host work).
- [ ] Clone repository and run `scripts/setup-dev-env.sh` to provision dependencies.
- [ ] Ensure IDE/editor uses `.editorconfig` settings; enable C# analyzers.
- [ ] Optionally install Avalonia tooling/extensions for preferred IDE.

## Knowledge Ramp-Up
- [ ] Read `ARCHITECTURE.md` and `docs/ENGINEERING-HANDBOOK.md`.
- [ ] Review execution plan milestone relevant to the onboarding timeline.
- [ ] Walk through sample app (`samples/TextEdit.Sandbox`) to understand control embedding.
- [ ] Pair with a core engineer to review coding conventions and PR expectations.

## Tooling & Workflow Familiarization
- [ ] Run `dotnet build TextEdit.sln` and `dotnet test TextEdit.sln` (once tests exist).
- [ ] Execute baseline benchmark suite (`dotnet run -c Release --project tests/benchmarks/TextEdit.Benchmarks`).
- [ ] Create first documentation ADR or feature spec using templates (as applicable).
- [ ] Submit a small starter PR (typo, doc fix, or simple bug) to validate workflow.

## Optional / Role-Specific Tasks
- [ ] Rendering-focused contributors: review `TextEdit.Rendering` project layout and performance goals.
- [ ] Language services contributors: set up sample LSP server integration environment.
- [ ] Extension ecosystem contributors: install Node tooling, run prototype extension host harness.

> Keep this checklist updated as new tooling or processes emerge.
