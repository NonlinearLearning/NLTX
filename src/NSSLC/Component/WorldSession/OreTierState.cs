namespace Terraria.WorldSession.Components;

public readonly record struct OreTierState(
  int Copper,
  int Iron,
  int Silver,
  int Gold,
  int Cobalt,
  int Mythril,
  int Adamantite)
{
  public static OreTierState Uninitialized => new(
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1);
}
