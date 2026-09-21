namespace Terraria.ClientPresentation.Ui.World;

public static class UiRarityQuery
{
  public static uint GetColor(int rarity)
  {
    return rarity switch
    {
      0 => 0xffffffff,
      1 => 0xff00ff00,
      2 => 0xff0080ff,
      3 => 0xffc000ff,
      _ => 0xffffffff
    };
  }

  public static bool TryGet(int rarity, out Snapshot snapshot)
  {
    bool known = rarity is >= 0 and <= 3;
    snapshot = new Snapshot(rarity, GetColor(rarity), known);
    return known;
  }

  public readonly record struct Snapshot(
    int Rarity,
    uint Color,
    bool IsKnown);
}
