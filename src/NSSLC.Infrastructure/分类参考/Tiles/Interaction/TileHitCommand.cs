namespace Terraria.Tiles.Interaction;

public readonly record struct TileHitCommand
{
  public enum ActionKind
  {
    RegisterHit,
    AddDamage,
    UpdatePosition,
    Clear,
    ClearAtLocation
  }

  private TileHitCommand(
    ActionKind action,
    int x,
    int y,
    TileHitKind hitKind,
    int damage,
    int newX,
    int newY)
  {
    Action = action;
    X = x;
    Y = y;
    HitKind = hitKind;
    Damage = damage;
    NewX = newX;
    NewY = newY;
  }

  public ActionKind Action { get; }

  public int X { get; }

  public int Y { get; }

  public TileHitKind HitKind { get; }

  public int Damage { get; }

  public int NewX { get; }

  public int NewY { get; }

  public static TileHitCommand AddDamage(int x, int y, TileHitKind hitKind, int damage)
  {
    return new(ActionKind.AddDamage, x, y, hitKind, damage, 0, 0);
  }

  public static TileHitCommand ClearAll()
  {
    return new(ActionKind.Clear, 0, 0, TileHitKind.Unused, 0, 0, 0);
  }

  public static TileHitCommand ClearAt(int x, int y, TileHitKind hitKind = TileHitKind.Unused)
  {
    return new(ActionKind.ClearAtLocation, x, y, hitKind, 0, 0, 0);
  }

  public static TileHitCommand Register(int x, int y, TileHitKind hitKind, int damage)
  {
    return new(ActionKind.RegisterHit, x, y, hitKind, damage, 0, 0);
  }

  public static TileHitCommand UpdatePosition(
    int x,
    int y,
    TileHitKind hitKind,
    int newX,
    int newY)
  {
    return new(ActionKind.UpdatePosition, x, y, hitKind, 0, newX, newY);
  }
}
