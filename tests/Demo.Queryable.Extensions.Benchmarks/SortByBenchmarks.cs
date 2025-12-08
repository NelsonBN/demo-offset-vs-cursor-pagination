using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace Demo.Queryable.Extensions.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[InProcess]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SortByBenchmarks
{
    private IQueryable<TestPerson> _data = new List<TestPerson>().AsQueryable();


    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Ascending")]
    public void SortBy_Name_Ascending_Using_Linq()
    {
        _ = _data.OrderBy(p => p.Name);
    }

    [Benchmark]
    [BenchmarkCategory("Ascending")]
    public void SortBy_Name_Ascending_Using_SortByExtension()
    {
        _ = _data.SortBy("Name");
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Descending")]
    public void SortBy_Age_Descending_Using_Linq()
    {
        _ = _data.OrderByDescending(p => p.Age);
    }

    [Benchmark]
    [BenchmarkCategory("Descending")]
    public void SortBy_Age_Descending_Using_SortByExtension()
    {
        _ = _data.SortBy("-Age");
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedVsUntyped")]
    public void SortBy_Typed()
    {
        var sort = "-Age".ParseSort<TestPerson>();
        _ = _data.SortBy(sort);
    }

    [Benchmark]
    [BenchmarkCategory("TypedVsUntyped")]
    public void SortBy_Untyped()
    {
        _ = _data.SortBy("-Age");
    }
}
