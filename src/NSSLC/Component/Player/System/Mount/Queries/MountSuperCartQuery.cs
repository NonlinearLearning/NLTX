namespace Terraria.Player.Mount;

public static class MountSuperCartQuery
{
  public static MountEffectiveMovementSnapshot Project(
    MountMovementDefinition movement,
    bool isUsingSuperCart)
  {
    if (!isUsingSuperCart)
    {
      return new MountEffectiveMovementSnapshot(
        movement.RunSpeed,
        movement.DashSpeed,
        movement.SwimSpeed,
        movement.Acceleration,
        movement.JumpSpeed,
        movement.JumpHeight,
        false);
    }

    MountSuperCartDefinition superCart = MountSuperCartDefinition.Version4;
    return new MountEffectiveMovementSnapshot(
      superCart.RunSpeed,
      superCart.DashSpeed,
      movement.SwimSpeed,
      superCart.Acceleration,
      superCart.JumpSpeed,
      superCart.JumpHeight,
      true);
  }
}
