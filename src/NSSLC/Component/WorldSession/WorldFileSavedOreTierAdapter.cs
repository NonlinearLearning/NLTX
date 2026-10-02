using System;

namespace Terraria.WorldSession.Components;

/// <summary>
/// Performs the evidence-backed saved-tier version and layout mapping.
/// </summary>
public sealed class WorldFileSavedOreTierAdapter : IWorldFileSavedOreTierAdapter
{
  public const int FirstFourDirectReadVersion = 216;
  public const int HighTiersDirectReadVersion = 54;
  public const int HighTierAltarFallbackVersion = 23;

  public OreTierState Read(in WorldSavedOreTierFileInput input)
  {
    int copper = input.VersionNumber >= FirstFourDirectReadVersion
      ? RequireField(input.Copper, nameof(input.Copper))
      : -1;
    int iron = input.VersionNumber >= FirstFourDirectReadVersion
      ? RequireField(input.Iron, nameof(input.Iron))
      : -1;
    int silver = input.VersionNumber >= FirstFourDirectReadVersion
      ? RequireField(input.Silver, nameof(input.Silver))
      : -1;
    int gold = input.VersionNumber >= FirstFourDirectReadVersion
      ? RequireField(input.Gold, nameof(input.Gold))
      : -1;

    (int cobalt, int mythril, int adamantite) =
      ReadHighTiers(input);

    return new OreTierState(
      copper,
      iron,
      silver,
      gold,
      cobalt,
      mythril,
      adamantite);
  }

  public WorldSavedOreTierFileSaveValues PrepareSave(
    in OreTierState state)
  {
    return new WorldSavedOreTierFileSaveValues(
      state.Cobalt,
      state.Mythril,
      state.Adamantite,
      state.Copper,
      state.Iron,
      state.Silver,
      state.Gold);
  }

  private static (int Cobalt, int Mythril, int Adamantite) ReadHighTiers(
    in WorldSavedOreTierFileInput input)
  {
    if (input.VersionNumber >= HighTiersDirectReadVersion)
    {
      return (
        RequireField(input.Cobalt, nameof(input.Cobalt)),
        RequireField(input.Mythril, nameof(input.Mythril)),
        RequireField(input.Adamantite, nameof(input.Adamantite)));
    }

    if (input.VersionNumber >= HighTierAltarFallbackVersion &&
        input.AltarCount == 0)
    {
      return (-1, -1, -1);
    }

    return (
      WorldSavedOreTierDefaults.Cobalt,
      WorldSavedOreTierDefaults.Mythril,
      WorldSavedOreTierDefaults.Adamantite);
  }

  private static int RequireField(int? value, string parameterName)
  {
    return value ?? throw new ArgumentException(
      "A directly read saved-tier field is required for this world-file version.",
      parameterName);
  }
}
