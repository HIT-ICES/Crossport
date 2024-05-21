using System;
using System.Collections.Generic;
using System.Linq;
using Anonymous.Crossport.Settings;
using Newtonsoft.Json;

namespace Anonymous.Crossport.Diagnostics
{
    [Serializable]
    public class Stats
    {
        [JsonProperty] public double min;
        [JsonProperty] public double max;
        [JsonProperty] public double average;
        [JsonProperty] public List<double> raw;

        public static Stats FromRaw(List<double> raw)
        {
            if (raw.Count == 0) return new();
            return new()
                   {
                       min = raw.Min(),
                       max = raw.Max(),
                       average = raw.Average(),
                       raw = raw
                   };
        }

        public static Stats FromRawAndAvg(List<double> raw, double average)
        {
            if (raw.Count == 0) return new();
            return new()
                   {
                       min = raw.Min(),
                       max = raw.Max(),
                       average = average,
                       raw = raw
                   };
        }
    }

    [Serializable]
    public class ClientExpStats
    {
        [JsonProperty] public Stats netLatency;
        [JsonProperty] public Stats mtpLatency;
        [JsonProperty] public Stats frameRate;
    }
}