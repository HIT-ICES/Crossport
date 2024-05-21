using Microsoft.AspNetCore.Mvc.Routing;

namespace Anonymous.Crossport.Utils;

public sealed class HttpConnectAttribute : HttpMethodAttribute
{
    private static readonly IEnumerable<string> SupportedMethods = new[] { "CONNECT" };

    public HttpConnectAttribute()
        : base(SupportedMethods) { }

    public HttpConnectAttribute(string template)
        : base(SupportedMethods, template) { }
}