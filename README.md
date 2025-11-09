# TextEdit

TextEdit is a fully virtualized, extensible code editing control for Avalonia 11.3.x and .NET 8. It targets IDE-scale workloads, multi-gigabyte documents, and reuse of VS Code-class language intelligence while staying idiomatic to Avalonia.

## Table of Contents
- [Overview](#overview)
- [Key Capabilities](#key-capabilities)
- [Architecture at a Glance](#architecture-at-a-glance)
- [Repository Layout](#repository-layout)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Build](#build)
  - [Run the sandbox sample](#run-the-sandbox-sample)
- [Testing & Quality](#testing--quality)
- [Benchmarks](#benchmarks)
- [Documentation](#documentation)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)

## Overview
TextEdit provides the foundations for embedding a world-class code editor into Avalonia applications. The project combines a high-performance buffer model, GPU-accelerated rendering, and an extension-friendly architecture so host applications can deliver Visual Studio- or VS Code-class editing experiences without building an IDE from scratch.

## Key Capabilities
- Fully virtualized rendering pipeline designed to keep UI threads free and hit 120+ FPS targets on large files.
- Piece-tree based document model enabling fast snapshotting, undo/redo journaling, and efficient diffing.
- Extensible language intelligence layer intended to host LSP clients, TextMate grammars, diagnostics, and semantic services.
- Plugin infrastructure spanning managed (.NET) and Node-based extensions to reuse VS Code ecosystem investments.
- Accessibility-aware input stack supporting multi-caret editing, column selections, IME composition, and assistive tooling.
- Observability hooks for telemetry, diagnostics, and performance instrumentation throughout the editor stack.

## Architecture at a Glance
- **Control Surface** (`CodeEditorControl`) orchestrates composition of rendering, input, and document services inside Avalonia.
- **Editor Kernel** offers headless document operations, caret/selection management, command routing, and change notifications.
- **Rendering Pipeline** builds on Avalonia `TextLayout`/`TextShaper` with viewport-aware scheduling, glyph caching, minimap, and overlay adornments.
- **Language Intelligence** layer targets LSP, TextMate, semantic tokens, diagnostics UI, and code actions via background workers.
- **Extension Host** plans for sandboxed managed and Node processes with a messaging bridge that mirrors VS Code extensibility.
- **Service Fabric** provides background job orchestration, caching, and telemetry to maintain responsiveness.
Detailed architectural goals, design milestones, and future work items live in [`ARCHITECTURE.md`](ARCHITECTURE.md).

## Repository Layout
- `src/` — Core libraries (`TextEdit.Core`, `TextEdit.Controls`, `TextEdit.Rendering`, `TextEdit.Extensions`) targeting `net8.0`.
- `samples/TextEdit.Sandbox/` — Minimal Avalonia host that wires up the editor control for manual exploration.
- `tests/unit/` — xUnit test projects covering core services and controls with `coverlet` instrumentation.
- `tests/benchmarks/` — BenchmarkDotNet harnesses focused on buffer and rendering hot paths.
- `docs/` — Engineering handbook, onboarding checklist, and documentation templates.
- `scripts/` — Automation helpers, including `setup-dev-env.sh` for provisioning local environments.

## Getting Started

### Prerequisites
- .NET 8 SDK (required)
- Node.js 18+ (recommended for developing the VS Code-compatible extension host)
- Git and supported build tools for your platform

Run `scripts/setup-dev-env.sh` to verify prerequisites, restore NuGet packages, and build the solution:

```bash
./scripts/setup-dev-env.sh
```

### Build
You can manually build the solution from the repository root:

```bash
dotnet restore TextEdit.sln
dotnet build TextEdit.sln --configuration Debug
```

### Run the sandbox sample
Launch the Avalonia sandbox to experiment with the editor surface:

```bash
dotnet run --project samples/TextEdit.Sandbox/TextEdit.Sandbox.csproj
```

## Testing & Quality
- Run all unit tests:

  ```bash
  dotnet test TextEdit.sln
  ```

- Coverage data is collected via the `coverlet.collector` package; configure collectors in `tests/unit/*/*.runsettings` (if provided) or pass `--collect:"XPlat Code Coverage"` when invoking `dotnet test`.
- Continuous test and performance strategy is outlined in `docs/ENGINEERING-HANDBOOK.md`.

## Benchmarks
Performance-critical routines use BenchmarkDotNet harnesses:

```bash
dotnet run --configuration Release --project tests/benchmarks/TextEdit.Benchmarks/TextEdit.Benchmarks.csproj
```

Review results under `tests/benchmarks/TextEdit.Benchmarks/bin/` to track regressions.

## Documentation
- [`ARCHITECTURE.md`](ARCHITECTURE.md) — Full architecture specification, subsystem design, and milestone tracking.
- [`docs/ENGINEERING-HANDBOOK.md`](docs/ENGINEERING-HANDBOOK.md) — Engineering practices, success metrics, and team roles.
- [`docs/ONBOARDING-CHECKLIST.md`](docs/ONBOARDING-CHECKLIST.md) — Step-by-step onboarding guidance for new contributors.
- `docs/templates/` — Templates for specs, RFCs, and design reviews.

## Roadmap
Development is organized into phased milestones captured in the architecture specification, covering:
- **Rendering & virtualization** hardening for large documents and high-DPI displays.
- **Language intelligence enablement** with LSP, TextMate, semantic tokens, and formatting hooks.
- **Extension ecosystem** work for managed and Node hosts plus marketplace packaging.
- **Streaming, diff/merge, and collaboration** scenarios built on the snapshot document graph.
- **Quality & observability** including instrumentation dashboards, watchdogs, and extensive automated testing.

Refer to the milestone tables in [`ARCHITECTURE.md`](ARCHITECTURE.md) for progress tracking and remaining work.

## Contributing
Contributions are welcome. Before opening a pull request:
- Align with the architectural direction documented in `ARCHITECTURE.md`.
- Run `./scripts/setup-dev-env.sh --verify-only` and ensure `dotnet test` passes.
- Include relevant documentation updates in `docs/` when introducing new subsystems or workflows.
- Use issues to discuss large features or architectural changes ahead of implementation.

## License
This project is licensed under the [MIT License](LICENSE).
