using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

public sealed record PlayerMovementInput(
  byte ControlFlags,
  byte MovementFlags,
  byte PlayerFeatureFlags,
  byte ActionFlags,
  byte SelectedItem,
  PacketVector2 Position,
  PacketVector2 Velocity,
  bool VelocityIncluded,
  ushort? MountType,
  PacketVector2? PotionOfReturnUsePosition,
  PacketVector2? PotionOfReturnHomePosition,
  PacketVector2? CameraTarget)
{
  public const float MaximumCoordinateMagnitude = 1_000_000f;

  public static PlayerMovementInput Initial => new(
    0,
    0,
    0,
    0,
    0,
    default,
    default,
    false,
    null,
    null,
    null,
    null);

  public static bool TryCreate(
    PlayerControlsPacket packet,
    bool preserveServerPosition,
    PlayerMovementInput current,
    out PlayerMovementInput movement)
  {
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(current);
    PacketVector2 position = preserveServerPosition ? current.Position : packet.Position;
    PacketVector2 velocity = preserveServerPosition ? current.Velocity : packet.Velocity ?? default;
    if (!IsValid(position) || !IsValid(velocity)
      || packet.PotionOfReturnUsePosition is PacketVector2 usePosition && !IsValid(usePosition)
      || packet.PotionOfReturnHomePosition is PacketVector2 homePosition && !IsValid(homePosition)
      || packet.CameraTarget is PacketVector2 cameraTarget && !IsValid(cameraTarget))
    {
      movement = current;
      return false;
    }

    movement = new PlayerMovementInput(
      packet.ControlFlags,
      packet.MovementFlags,
      packet.PlayerFeatureFlags,
      packet.ActionFlags,
      packet.SelectedItem,
      position,
      velocity,
      packet.Velocity is not null,
      packet.MountType,
      packet.PotionOfReturnUsePosition,
      packet.PotionOfReturnHomePosition,
      packet.CameraTarget);
    return true;
  }

  private static bool IsValid(PacketVector2 value)
  {
    return float.IsFinite(value.X)
      && float.IsFinite(value.Y)
      && MathF.Abs(value.X) <= MaximumCoordinateMagnitude
      && MathF.Abs(value.Y) <= MaximumCoordinateMagnitude;
  }
}
