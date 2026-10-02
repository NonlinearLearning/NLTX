namespace Terraria.NpcTownBestiary;

public sealed class BestiaryFilterDefinition
{
  public BestiaryFilterDefinition(
    BestiaryFilterKind kind,
    bool? forcedDisplay,
    string? infoElementKey = null)
  {
    if (kind == BestiaryFilterKind.InfoElement && string.IsNullOrWhiteSpace(infoElementKey))
    {
      throw new ArgumentException(
        "An info element filter requires a key.",
        nameof(infoElementKey));
    }

    Kind = kind;
    ForcedDisplay = forcedDisplay;
    InfoElementKey = infoElementKey;
  }

  public BestiaryFilterKind Kind { get; }

  public bool? ForcedDisplay { get; }

  public string? InfoElementKey { get; }
}
