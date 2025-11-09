# Benchmark Results

Benchmarks should run in Release configuration on pinned hardware; update this document with summaries after each milestone-level tuning effort.

## Reporting Workflow
- Run `dotnet run -c Release --project tests/benchmarks/TextEdit.Benchmarks` on representative hardware.
- Capture BenchmarkDotNet summary tables and append to this document.
- Record environment metadata (CPU, memory, OS, .NET SDK) and link to raw `.md` or `.log` outputs.

## Target Scenarios
- Core document buffer operations (insert, delete, snapshot clone).
- Rendering pipeline primitive benchmarks (layout measurement, glyph cache interactions).
- Language service roundtrip simulations (mock LSP server latency microbenchmarks).

## Baseline Snapshot (2025-01-07)
- Host: macOS Sequoia 15.6, Apple M3 Pro, .NET SDK 10.0.100-rc.2.25502.107.
- Command: `dotnet run --project tests/benchmarks/TextEdit.Benchmarks/TextEdit.Benchmarks.csproj --configuration Release -- --filter *EditorKernel* --iterationCount 1 --warmupCount 1`
- Results:
  - `EditorKernelBenchmarks.Access EditorKernel version` → Mean `0.153 ns`, Alloc `0 B`.

> Replace this section with richer tables once substantive benchmarks are implemented.
