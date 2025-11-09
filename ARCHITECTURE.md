# TextEdit Architecture Specification

## 1. Vision and Goals
- Deliver a first-class, high-performance, fully virtualized text editing control targeting Avalonia 11.3.x and optimized for code editing scenarios at IDE scale.
- Provide deep extensibility so features can be composed into Visual Studio- or VS Code-class editing experiences, including reuse of existing VS Code language services and extensions.
- Support interactive editing of extremely large documents (gigabytes) and streaming/remote content with bounded memory usage and smooth UI at 120+ FPS.
- Maintain platform neutrality across desktop and embedded targets while remaining idiomatic to Avalonia’s control model and threading rules.

## 2. System Overview
- **Control Surface (`CodeEditorControl`)** — an Avalonia `Control` exposing the core editing surface, orchestrating composition of rendering, input, and document services.
- **Editor Kernel** — headless core providing document model, diffing, undo/redo, caret/selection, structural navigation, and command routing.
- **Rendering Pipeline** — GPU-accelerated text/inline element renderer with virtualization-aware layout scheduler built on Avalonia’s `TextLayout`/`TextShaper`.
- **Language Intelligence Layer** — LSP client host, TextMate grammar engine, inline diagnostics, code actions, and semantic tokens.
- **Extension Host** — plugin framework supporting managed (.NET) and Node-based VS Code extensions via a sandboxed host process with messaging bridge.
- **Service Fabric** — background job system, caching layers, telemetry, and diagnostics instrumentation to keep UI thread free of heavy work.

## 3. Document and Data Management
- **Piece-Tree Text Buffer** inspired by VS Code’s `PieceTreeTextBuffer` for O(1) snapshot cloning, fast insert/delete, and incremental diffing.
- **Document Graph** with `DocumentId`, `DocumentSnapshot`, and `DocumentVersion` abstractions enabling multi-view, projections (e.g., diff view), and collaborative scenarios.
- **Virtualized Line Model** storing logical lines as immutable records with lightweight metadata caches (token spans, folding regions, git blame data) that hydrate on demand.
- **Chunked Storage Providers** supporting:
  - Local file memory-mapped segments for large files.
  - Streaming providers (HTTP, IPC, git blob) with adaptive prefetch and back-pressure.
  - In-memory transient buffers for ephemeral documents (scratch pads, REPL).
- **Undo/Redo Journal** using intent-based operations (insert block, change indentation) with compression and coalescing tuned for continuous typing.
- **Change Notification Bus** (observable events) used by rendering, extensions, and analytics to react without tight coupling.

## 4. Layout and Rendering Pipeline
- **Viewport-Aware Layout Scheduler** calculates visible line ranges via pixel-perfect scroll offsets and requests layout work from a background worker that populates a line cache.
- **Text Layout Virtualization** only shapes glyphs for visible lines +/- cache window; leverages glyph atlas caching (Skia/Avalonia) and retains per-font/language caches.
- **Inline Element Pipeline** supporting adorners (errors, code lenses, bookmarks) with z-ordered layering and virtualized anchors (relative to document positions, not lines).
- **Composite Rendering Graph**:
  - Background layer: theme-aware brush tiling, minimap preview.
  - Text layer: GPU text draw using batched `DrawingContext`.
  - Overlay layer: selections, carets, find highlights.
  - Adornment layer: inline diagnostics, folding widgets.
- **Async Decoration Resolver** ensures expensive adornments (semantic highlights, git authorship) hydrate off the UI thread and reuse the virtualization window.
- **High-DPI & Variable Font Support** inheriting Avalonia 11.3 text shaping, with fallback fonts and emoji layers.
- **Minimap & Overview Ruler** rendered via simplified glyph metrics and heat-map overlays using the same document snapshots.

## 5. Input, Commands, and UX Infrastructure
- **Command Router** built atop Avalonia’s `RoutedCommand` with context-aware command tables (core editor, selection, multi-caret, structural editing).
- **Input Abstraction** translating pointer/keyboard/IME events into editor gestures; supports multi-caret, column selection, touch, stylus, and accessibility interactions.
- **Caret & Selection Manager** managing logical/visual carets, bidi text, rectangular selections, and virtualization of caret adornments.
- **IME & Composition Support** leveraging Avalonia IME APIs with composition region tracking inside the virtualized line cache.
- **Search & Navigation Services**: incremental search, fuzzy symbol navigation, multi-buffer search running on background threads with progress UI.

## 6. Language Intelligence Integration
- **LSP Client Host** modeled after VS Code’s `vscode-languageclient`, running in a dedicated worker with JSON-RPC transport; supports multiplexing multiple documents and servers.
- **TextMate Grammar Engine** reusing `AvaloniaEdit.TextMate` or `Oniguruma`-based tokenizers with incremental parsing and token diff caching.
- **Semantic Tokens & Folding** applying results onto line metadata caches; virtualization ensures only visible regions are hydrated.
- **Diagnostics Pipeline** merging server diagnostics, build output, and analyzers; results feed adornments, overview ruler marks, and problem lists.
- **Code Actions & Refactorings** integrated via command router with preview UI hosted inside flyouts/panels.
- **Inline Hints & Lenses** with asynchronous providers, caching, and virtualization to avoid layout thrashing.
- **Formatting & Code Style** supporting range formatting, on-type formatting, and integration with external formatters (e.g., `dotnet format`, `prettier`).

## 7. Extension and Plugin Architecture
- **Extension Manifest** describing contributions (commands, menus, keybindings, language grammars, themes, service providers).
- **Managed Extension SDK** offering dependency injection, lifecycle hooks (`OnActivate`, `OnDeactivate`), and service registration (document listeners, content providers).
- **Node/VS Code Compatibility Layer** embedding a lightweight Node host to load VS Code extensions:
  - Uses the `@coding-editor/host` shim replicating VS Code extension API surface (subset) and mapping to core services.
  - Communicates via JSON-RPC over stdio/IPC with marshaled types for diagnostics, text edits, etc.
- **Isolation & Sandbox**: each extension runs in separate context with capability filters, rate limiting, and cancellation tokens to protect UI thread.
- **Composition Catalog** merges managed and JS extensions, resolves conflicts, and applies contribution points at runtime.
- **Marketplace/Deployment Strategy** for packaging extensions as `.zip`/NuGet bundles with dependency metadata.

## 8. Advanced Editing Features
- **Structural Editing**: AST-aware selection, block manipulation, multi-caret transformations powered by language services.
- **Diff & Merge View**: side-by-side and inline diff leveraging document snapshots and virtualization to only compute diff hunks in view.
- **Collaboration Hooks**: CRDT/Operational Transform ready document model for future live-share features.
- **Embedded Tools**: integrated terminal, REPL consoles, debug inline values via extension points.
- **Accessibility**: screen reader support via UIA providers, high-contrast themes, caret tracking, and keyboard-first workflows.

## 9. Performance and Reliability Strategy
- **Zero-Allocation Inner Loop**: pooling, span-based operations, and SIMD-friendly text scanning for hot paths (search, tokenization).
- **Threading Model**: UI thread only schedules rendering; background worker pool handles parsing, LSP, I/O. Central `Scheduler` enforces priority and cancellation.
- **Instrumentation**: diagnostic overlay (FPS, layout time, LSP latency), ETW/EventPipe hooks, and integration with `ActivitySource`.
- **Resilience**: watchdogs for hung extensions, bounded memory caches with LRU policies, automatic recovery for corrupted documents.
- **Regression Testing**: microbenchmarks (BenchmarkDotNet), soak tests (typing simulation), snapshot-based rendering assertions, fuzzing for document mutations.

## 10. Streaming and Remote Data Scenarios
- **Data Source Abstraction** supporting git blobs, HTTP ranges, SSH/FTP, databases.
- **Speculative Fetching** based on scroll direction and caret velocity, with cancellation.
- **Partial Document Mode**: display placeholder gutters for unloaded regions, update UI as chunks arrive, maintain stable anchors for selections.
- **Offline Cache** storing compressed snapshots with validation hashes to support reconnect/resume scenarios.
- **Conflict Resolution** for remote edits using diff/merge strategies with extension hooks.

## 11. Integration with Avalonia Ecosystem
- **Styling/Theming** aligned with Avalonia’s `FluentTheme` and custom palette injection; exposes resource keys for easy theming.
- **Control Templates** enabling composition inside MVVM apps; provides design-time metadata for previewers.
- **Interop with AvaloniaEdit/HexView**:
  - Borrow `AvaloniaEdit`’s text view abstractions for compatibility layers/migration.
  - Adopt `HexView`’s virtualization concepts (segment caching, asynchronous measurement) for scrolling and viewport management.
- **Tooling Support**: design-time stub provider, XAML sample data, and Roslyn analyzers for extension authors.

## 12. Security and Privacy Considerations
- Enforce extension sandbox policies, file system access scopes, and secure secret handling for language servers.
- Provide opt-in telemetry with anonymization and GDPR compliance.
- Harden IPC channels (auth tokens, TLS for remote language servers).

---

## Execution Plan

### Milestone 1 – Foundations & Tooling Readiness
1. [x] Confirm scope, success metrics, and initial staffing; publish engineering handbook stub.
2. [x] Scaffold solution structure (`TextEdit.Core`, `TextEdit.Rendering`, `TextEdit.Extensions`, sample app).
3. [x] Configure build pipeline (GitHub Actions/Azure DevOps), code-quality gates, and dependency management (NuGet feeds, Renovate).
4. [x] Establish coding conventions, analyzer rulesets, and documentation templates.
5. [x] Set up benchmarking harness (BenchmarkDotNet) and performance baselines for reference implementations.
6. [x] Create developer onboarding checklist and environment provisioning scripts.

### Milestone 2 – Text Buffer & Document Graph
1. [x] Implement core `PieceTree` buffer with Unicode-aware insert/delete, snapshot cloning, and chunk persistence.
2. [x] Add undo/redo journal with coalescing strategies for typing, paste, and multi-caret edits.
3. [x] Define document identity model (`DocumentId`, `DocumentSnapshot`, `DocumentVersion`) and lifecycles.
4. [ ] Build change notification bus (pub/sub events, transaction batching, throttling).
5. [x] Introduce projection buffers (read-only, diff, metadata overlays) and validate multi-view consistency.
6. [x] Write unit tests covering edge cases (large files, CRLF normalization, surrogate pairs).

### Milestone 3 – Virtualization, Layout, and Rendering Core
1. [x] Develop viewport manager and scroll state computation with pixel-accurate line windowing.
2. [x] Implement asynchronous layout scheduler populating virtualized line cache with prefetch windowing.
3. [x] Integrate glyph atlas caching and GPU text drawing pipeline leveraging Avalonia 11.3 features.
4. [x] Add background/overlay/adornment render layers with invalidation budgeting.
5. [x] Create minimap/overview ruler prototypes sharing document snapshots.
6. [x] Benchmark rendering throughput on large files and adjust cache sizing heuristics.

### Milestone 4 – Editing UX & Interaction Framework
1. [x] Implement caret/selection manager covering single, multi-caret, column selection, and bidi scenarios.
2. [ ] Wire Avalonia input events to gesture abstraction layer (keyboard, mouse, touch, stylus, accessibility).
3. [ ] Build IME composition surfaces with pre-edit rendering inside virtualized lines.
4. [ ] Introduce command router with routed commands, command palette, and configurable keymaps.
5. [ ] Deliver search/navigation services (incremental find, go to line/symbol) using background execution.
6. [ ] Harden undo/redo, clipboard, and drag/drop workflows with integration tests.

### Milestone 5 – Language Intelligence Enablement
1. [ ] Integrate TextMate tokenization service with incremental token diffing and theme bindings.
2. [ ] Implement LSP client host (JSON-RPC transport, lifecycle management, diagnostics wiring).
3. [ ] Deliver completions, hovers, signature help, and diagnostics UI roundtrips end-to-end.
4. [ ] Add semantic tokens, folding, and code lens providers respecting virtualization budgets.
5. [ ] Provide formatting hooks (range/on-type) and configurable formatter registry.
6. [ ] Validate multi-language (C#, TypeScript, JSON) scenarios with automated integration tests.

### Milestone 6 – Extensions, Streaming, and Advanced Views
1. [ ] Design extension manifest schema and managed extension SDK with DI entry points.
2. [ ] Embed Node-based host for VS Code extensions, implement capability sandbox, and IPC bridge.
3. [ ] Support extension contribution points (commands, menus, grammars, themes) with conflict resolution.
4. [ ] Implement streaming data providers (memory-mapped files, HTTP range, git blob) with adaptive prefetch.
5. [ ] Ship diff/merge views leveraging snapshot model and virtualized rendering reuse.
6. [ ] Enable collaboration hooks (OT/CRDT abstraction) and document change reconciliation APIs.

### Milestone 7 – Quality, Observability, and Release Preparation
1. [ ] Add instrumentation dashboard (FPS, layout, LSP latency) and logging/metrics exporters.
2. [ ] Implement watchdogs for extension/language server hangs and memory pressure limits.
3. [ ] Build full test matrix (unit, property, fuzzing, UI automation, long-run soak) integrated into CI.
4. [ ] Conduct accessibility review, implement UIA providers, screen reader announcements, high-contrast palettes.
5. [ ] Produce developer tooling: sample apps, walkthroughs, API docs, extension author guide.
6. [ ] Package extension marketplace artifacts, signing, versioning policy, and release checklist.
