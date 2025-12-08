using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Demo.Queryable.Extensions.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[InProcess]
public class EncodeCursorBenchmarks
{
    private const string KeyValue = "KeyValue";
    private const string TargetValue = "TargetValue";

    [Benchmark]
    public void Encode_OnlyKeyValue()
    {
        _ = CursorUtils.Encode(KeyValue);
    }

    [Benchmark]
    public void Encode_KeyValueAndTargetValue()
    {
        _ = CursorUtils.Encode(KeyValue, TargetValue);
    }

    [Benchmark]
    public void Encode_KeyValueAndTargetValueNull()
    {
        _ = CursorUtils.Encode(KeyValue, null);
    }
}
