namespace Ices.Crossport.Core.Entities;

[Serializable]
public record AppConfig(
    int ResolutionX,
    int ResolutionY,
    int FrameRate,
    int QualityLevel,
    string? Profile)
{
    public static AppConfig Default { get; } = new(2560, 2560, 60, 5, null);
}