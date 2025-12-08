using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace Demo.Queryable.Extensions.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[InProcess]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SortByAndProjectionBenchmarks
{
    [Params(100, 1_000, 10_000, 100_000)]
    public int Size;

    private IQueryable<TestPerson> _data;

    [GlobalSetup]
    public void Setup()
    {
        var data = new List<TestPerson>();
        for (var i = 0; i < Size; i++)
        {
            data.Add(new(
                Guid.NewGuid(),
                $"Name {i}",
                (uint)(i % 100),
                i % 2 == 0,
                i * 1000.0 + 0.99));
        }
        _data = data.AsQueryable();
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Ascending")]
    public void SortBy_Name_Ascending_Using_Linq()
    {
        _ = _data.OrderBy(p => p.Name).ToList();
    }

    [Benchmark]
    [BenchmarkCategory("Ascending")]
    public void SortBy_Name_Ascending_Using_SortByExtension()
    {
        _ = _data.SortBy("Name").ToList();
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Descending")]
    public void SortBy_Age_Descending_Using_Linq()
    {
        _ = _data.OrderByDescending(p => p.Age).ToList();
    }

    [Benchmark]
    [BenchmarkCategory("Descending")]
    public void SortBy_Age_Descending_Using_SortByExtension()
    {
        _ = _data.SortBy("-Age").ToList();
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedVsUntyped")]
    public void SortBy_Typed()
    {
        var sort = "-Age".ParseSort<TestPerson>();
        _ = _data.SortBy(sort).ToList();
    }

    [Benchmark]
    [BenchmarkCategory("TypedVsUntyped")]
    public void SortBy_Untyped()
    {
        _ = _data.SortBy("-Age").ToList();
    }
}
