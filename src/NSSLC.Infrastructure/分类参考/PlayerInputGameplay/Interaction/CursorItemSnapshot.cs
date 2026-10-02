namespace NLTX.PlayerInputGameplay.Interaction;

public readonly record struct CursorItemSnapshot(int Type, int Prefix, int Stack)
{
  public bool IsAir => Type == 0 || Stack <= 0;

  public static CursorItemSnapshot Air => new(0, 0, 0);
}
