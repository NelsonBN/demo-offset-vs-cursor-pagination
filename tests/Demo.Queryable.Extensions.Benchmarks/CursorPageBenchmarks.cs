using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace Demo.Queryable.Extensions.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[InProcess]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CursorPageBenchmarks
{
    private IQueryable<TestPerson> _data = new List<TestPerson>().AsQueryable();

    private static readonly string _emptyKeyStr = Guid.Empty.ToString();
    private static readonly string _keyStr = "2975ca59-cd2b-4726-b6a9-cff2c8668059";
    private static readonly Guid _key = Guid.Parse(_keyStr);

    private static readonly int _fieldValue = 22;
    private static readonly string _fieldValueStr = _fieldValue.ToString();


    [Benchmark(Baseline = true)]
    [BenchmarkCategory("NoCursorProvided")]
    public void NoCursorProvided_Using_Linq()
    {
        _data
            .OrderBy(p => p.Age)
            .ThenBy(s => s.Id);
    }

    [Benchmark]
    [BenchmarkCategory("NoCursorProvided")]
    public void NoCursorProvided_Using_CursorPageExtension()
    {
        _data.CursorPage(s => s.Id, "Age", null);
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("SortIsKey")]
    public void SortIsKey_Using_Linq()
    {
        _data
            .OrderBy(p => p.Id);
    }

    [Benchmark]
    [BenchmarkCategory("SortIsKey")]
    public void SortIsKey_Using_CursorPageExtension()
    {
        _data.CursorPage(s => s.Id, "Id", null);
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("DescendingSortIsNonKey")]
    public void DescendingSortIsNonKey_Using_Linq()
    {
        _data
            .OrderByDescending(p => p.Age)
            .ThenBy(s => s.Id);
    }

    [Benchmark]
    [BenchmarkCategory("DescendingSortIsNonKey")]
    public void DescendingSortIsNonKey_Using_CursorPageExtension()
    {
        _data.CursorPage(s => s.Id, "-Age", null);
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CursorProvided")]
    public void CursorProvided_Using_Linq()
    {
        _data
            .Where(p => p.Age > 26)
            .OrderBy(p => p.Age)
            .ThenBy(s => s.Id);
    }

    [Benchmark]
    [BenchmarkCategory("CursorProvided")]
    public void CursorProvided_Using_CursorPageExtension()
    {
        var cursor = new Cursor(
            _emptyKeyStr,
            "26");
        _data.CursorPage(s => s.Id, "Age", cursor);
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("FieldMatches")]
    public void FieldMatches_Using_Linq()
    {
        _data
            .Where(p =>
                p.Age > _fieldValue ||
                (p.Age == _fieldValue && p.Id.CompareTo(_key) > 0))
            .OrderBy(p => p.Age)
            .ThenBy(s => s.Id);
    }

    [Benchmark]
    [BenchmarkCategory("FieldMatches")]
    public void FieldMatches_Using_CursorPageExtension()
    {
        var cursor = new Cursor(
            _keyStr,
            _fieldValueStr);

        _data.CursorPage(s => s.Id, "Age", cursor);
    }



    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ComplexPage")]
    public void ComplexPage_Using_Linq()
    {
        _data
            .Where(p =>
                p.Age < _fieldValue ||
                (p.Age == _fieldValue && p.Id.CompareTo(_key) > 0))
            .OrderByDescending(p => p.Age)
            .ThenBy(s => s.Id);
    }

    [Benchmark]
    [BenchmarkCategory("ComplexPage")]
    public void ComplexPage_Using_CursorPageExtension()
    {
        var cursor = new Cursor(
            _keyStr,
            _fieldValueStr);

        _data.CursorPage(s => s.Id, "-Age", cursor);
    }
}
