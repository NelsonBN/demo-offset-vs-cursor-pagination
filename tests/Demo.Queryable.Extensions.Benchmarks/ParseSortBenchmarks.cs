using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Demo.Queryable.Extensions.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[InProcess]
public class ParseSortBenchmarks
{
    [Benchmark]
    public void EmptyString_WithDefault()
    {
        var result = "".ParseSort("id");
        _ = result.PropertyName;
        _ = result.IsAscending;
    }

    [Benchmark]
    public void SimpleProperty()
    {
        var result = "name".ParseSort();
        _ = result.PropertyName;
        _ = result.IsAscending;
    }

    [Benchmark]
    public void SimpleProperty_WithDefault()
    {
        var result = "name".ParseSort("id");
        _ = result.PropertyName;
        _ = result.IsAscending;
    }

    [Benchmark]
    public void AscendingProperty()
    {
        var result = "+name".ParseSort();
        _ = result.PropertyName;
        _ = result.IsAscending;
    }

    [Benchmark]
    public void DescendingProperty()
    {
        var result = "-name".ParseSort();
        _ = result.PropertyName;
        _ = result.IsAscending;
    }

    [Benchmark]
    public void TryParseSort_WithEmptyString()
    {
        var result = "".TryParseSort(out var sort);
    }

    [Benchmark]
    public void TryParseSort_AscendingProperty()
    {
        var result = "+name".TryParseSort(out var sort);
    }

    [Benchmark]
    public void TryParseSort_DescendingProperty()
    {
        var result = "-name".TryParseSort(out var sort);
    }

    [Benchmark]
    public void TryParseSort_WithType()
    {
        var result = "name".TryParseSort<TestPerson>(out var sort);
    }

    [Benchmark]
    public void ParseSort_WithType()
    {
        var result = "name".ParseSort<TestPerson>();
    }
}
