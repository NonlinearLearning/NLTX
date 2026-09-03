using System.Numerics;

namespace Terraria.Player;

public struct PlayerAimComponent
{
  public PlayerAimComponent(Vector2 direction)
  {
    Direction = direction;
  }

  public Vector2 Direction;
}
