using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcTargetGeometrySnapshot(
  Vector2 Position,
  int Width,
  int Height)
{
  public Vector2 Center => Position + new Vector2(Width / 2f, Height / 2f);
}
