using Ices.Crossport.Core.Entities;
using System.Text.Json.Serialization;

namespace Ices.Crossport.Core.Diagnostics;

[Serializable]
public record ExperimentConfig(
    int Users,
    int Index,
    Dictionary<string, AppConfig> Components
);

public interface IStats<out T> where T : struct
{
    T Min { get; }
    T Max { get; }
    T Average { get; }
    T[] Raw { get; }
}

[Serializable]
public record struct Latency(double Min, double Max, double Average, double[] Raw) : IStats<double>
{
    public static Latency FromRaw(List<double> raw)
    {
        if (raw.Count == 0) return new();
        return new(raw.Min(), raw.Max(), raw.Average(), raw.ToArray());
    }

    public static Latency Conclude(IEnumerable<Latency> origins)
        => FromRaw(origins.SelectMany(l => l.Raw).ToList());
}

[Serializable]
public record struct FrameRate(double Min, double Max, double[] Raw, uint FrameCount, long TimeDiff) : IStats<double>
{
    [JsonIgnore] public readonly double Average => TimeDiff == 0 ? 0 : FrameCount / ((double)TimeDiff / 1000000L);

    public static FrameRate FromRaw(List<double> raw, uint frameCount, long timeDiff)
    {
        if (raw.Count == 0) return new();
        return new(raw.Min(), raw.Max(), raw.ToArray(), frameCount, timeDiff);
    }

    public static FrameRate Conclude(IEnumerable<FrameRate> origins)
    {
        var list = origins.ToList();
        return FromRaw
        (
            list.SelectMany(f => f.Raw).ToList(),
            (uint)list.Sum(f => f.FrameCount),
            list.Sum(f => f.TimeDiff)
        );
    }
}

[Serializable]
public record struct ClientExpStats(
    Latency NetLatency,
    Latency MtpLatency,
    FrameRate LocalFrameRate,
    FrameRate RemoteFrameRate
);

[Serializable]
public record struct ExpStats(
    IStats<double> NetLatency,
    IStats<double> MtpLatency,
    IStats<double> LocalFrameRate,
    IStats<double> RemoteFrameRate
);

[Serializable]
public record struct ExperimentResult(
    string App,
    int Users,
    int Index,
    ExpStats Stats
);