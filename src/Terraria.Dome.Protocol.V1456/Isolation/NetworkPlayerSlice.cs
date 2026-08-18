using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public readonly record struct NetworkPlayerSlice(
  byte PlayerSlot,
  bool IsActive,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY,
  int Facing,
  int Health,
  int Mana,
  int MaximumMana)
{
  public static NetworkPlayerSlice From(PlayerSnapshot snapshot)
  {
    return new NetworkPlayerSlice(
      snapshot.AssignedSlot,
      snapshot.IsActive,
      snapshot.Position.X,
      snapshot.Position.Y,
      snapshot.Velocity.X,
      snapshot.Velocity.Y,
      snapshot.Facing,
      snapshot.Health,
      snapshot.Mana,
      snapshot.MaximumMana);
  }
}
