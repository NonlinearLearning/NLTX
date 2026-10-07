namespace Terraria.WorldSession.Components;

public static class WorldSavedOreTierDefaults
{
  public const int Copper = 7;
  public const int Iron = 6;
  public const int Silver = 9;
  public const int Gold = 8;
  public const int Cobalt = 107;
  public const int Mythril = 108;
  public const int Adamantite = 111;

  public static OreTierState Create()
  {
    return new OreTierState(
      Copper,
      Iron,
      Silver,
      Gold,
      Cobalt,
      Mythril,
      Adamantite);
  }
}
