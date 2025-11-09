using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using TextEdit.Core;

namespace TextEdit.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class EditorKernelBenchmarks
{
    [SuppressMessage("Performance", "CA1822", Justification = "BenchmarkDotNet requires instance benchmark methods by default.")]
    [Benchmark(Description = "Access EditorKernel version")]
    public string ReadKernelVersion() => EditorKernel.Version;
}
