﻿using System.Text.Json;

namespace Ices.Crossport.Utils;

public static class JsonHelper
{
    public static long ToJavascriptTimeStamp(this DateTime time)
    {
        var javascriptTimeStampZero = new DateTime(1970, 1, 1, 0, 0, 0);
        return (long)(time.ToUniversalTime() - javascriptTimeStampZero).TotalMilliseconds;
    }

    public static T? DeserializeWeb<T>(this JsonElement e)
    {
        return e.Deserialize<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public static string SafeGetString(this Dictionary<string, object> jsonObject, string fieldName)
    {
        if (!jsonObject.ContainsKey(fieldName)) return "";
        return jsonObject[fieldName].ToString() ?? "";
    }
}