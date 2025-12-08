using BenchmarkDotNet.Running;
using Demo.Queryable.Extensions.Benchmarks;

// BenchmarkRunner.Run<ParseSortBenchmarks>();
// BenchmarkRunner.Run<SortByBenchmarks>();
// BenchmarkRunner.Run<SortByAndProjectionBenchmarks>();
// BenchmarkRunner.Run<EncodeCursorBenchmarks>();
// BenchmarkRunner.Run<DecodeCursorBenchmarks>();
BenchmarkRunner.Run<CursorPageBenchmarks>();
