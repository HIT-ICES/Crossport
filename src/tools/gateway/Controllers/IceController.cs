using Microsoft.AspNetCore.Mvc;

namespace Anonymous.Crossport.Controllers;

[Route("ice")]
public class IceController(IConfiguration config) : Controller
{
    [HttpGet("public")]
    public IceConfig GetPublicStun()
    {
        var iceConf = config.GetSection("ICE");
        return new IceConfig
        (
            iceConf["Username"],
            iceConf["Password"],
            [
                $"turn:{iceConf["ExternalIp"]}:{iceConf["ExternalPort"]}?transport=udp",
                $"turn:{iceConf["ExternalIp"]}:{iceConf["ExternalPort"]}?transport=tcp"
            ]
        );
    }

    [HttpGet("private")]
    public IceConfig GetPrivateStun()
    {
        var iceConf = config.GetSection("ICE");
        return new IceConfig
        (
            iceConf["Username"],
            iceConf["Password"],
            [
                $"turn:{iceConf["InternalIp"]}:{iceConf["InternalPort"]}?transport=udp",
                $"turn:{iceConf["InternalIp"]}:{iceConf["InternalPort"]}?transport=tcp"
            ]
        );
    }

    [Serializable] public record IceConfig(string? Username, string? Credential, string[] Urls);
}