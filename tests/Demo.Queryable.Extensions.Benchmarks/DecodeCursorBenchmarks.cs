using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Demo.Queryable.Extensions.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[InProcess]
public class DecodeCursorBenchmarks
{
    private const string Encoded_KeyValue = "S2V5VmFsdWU";
    private const string Encoded_KeyValueAndTargetValue = "S2V5VmFsdWUAVGFyZ2V0VmFsdWU";

    [Benchmark]
    public void Decode_OnlyKeyValue()
    {
        _ = CursorUtils.Decode(Encoded_KeyValue);
    }

    [Benchmark]
    public void Decode_KeyValueAndTargetValue()
    {
        _ = CursorUtils.Decode(Encoded_KeyValueAndTargetValue);
    }
}
