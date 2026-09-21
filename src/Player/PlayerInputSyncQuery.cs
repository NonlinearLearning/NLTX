namespace Terraria.Player;

public static class PlayerInputSyncQuery
{
  public static PlayerInputSyncProjection Evaluate(
    in PlayerInputSyncCacheInput input)
  {
    bool pressingAnyInput = input.ControlLeft ||
      input.ControlRight ||
      input.ControlUp ||
      input.ControlDown ||
      input.ControlJump;

    return new PlayerInputSyncProjection(
      input.ControlLeft,
      input.ControlRight,
      input.ControlUp,
      input.ControlDown,
      input.ControlJump,
      pressingAnyInput);
  }
}
