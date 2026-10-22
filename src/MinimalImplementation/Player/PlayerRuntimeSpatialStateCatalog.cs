using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Version4 mount and collider calculations kept beside the Player runtime state.</summary>
public static class PlayerRuntimeSpatialStateCatalog
{
  public const byte MountCommandFlag = 0b1000_0000;

  /// <summary>
  /// Validates the packet-13 mount field and prepares the raw input facts for the collision
  /// owner. The current collider and position are intentionally preserved: mount sizing and
  /// bottom-center collision checks are derived by the world owner from that current hitbox.
  /// </summary>
  public static bool TryCreateCollisionCandidate(
    PlayerRuntimeSpatialState current,
    PlayerControlsPacket packet,
    PlayerMovementInput movement,
    out PlayerRuntimeSpatialState candidate,
    out PlayerMovementInput candidateMovement)
  {
    ArgumentNullException.ThrowIfNull(current);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(movement);
    bool mountCommand = (packet.MovementFlags & MountCommandFlag) != 0;
    if (mountCommand && (packet.MountType is not ushort requestedMount || requestedMount > 63))
    {
      candidate = current;
      candidateMovement = movement;
      return false;
    }

    candidate = current with
    {
      Pulley = (packet.MovementFlags & 0b0000_0001) != 0,
      VoidVaultEnabled = (packet.PlayerFeatureFlags & 0b0000_0010) != 0
    };
    candidateMovement = movement;
    return true;
  }

  public static bool TryApplyMovementCommand(
    PlayerRuntimeSpatialState current,
    PlayerControlsPacket packet,
    PlayerMovementInput movement,
    IPlayerRuntimeWorldCapture? world,
    out PlayerRuntimeSpatialState updated,
    out PlayerMovementInput updatedMovement,
    out bool unavailable)
  {
    ArgumentNullException.ThrowIfNull(current);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(movement);
    bool mountCommand = (packet.MovementFlags & MountCommandFlag) != 0;
    bool invalidMountCommand = mountCommand
      && (packet.MountType is not ushort requestedMount || requestedMount > 63);
    updated = current;
    updatedMovement = movement;
    unavailable = false;
    if (invalidMountCommand)
    {
      return false;
    }
    if (mountCommand && packet.MountType is ushort requestedType)
    {
      bool sameMount = current.MountActive && current.ActiveMountType == requestedType;
      if (requestedType == 5 && !sameMount && world?.IsWet is null)
      {
        unavailable = true;
        return false;
      }
      if (sameMount || requestedType != 5 || world!.IsWet == false)
      {
        if (!current.MountActive || current.ActiveMountType != requestedType)
        {
          PlayerRuntimeSpatialState mounted = CreateMountedState(current, requestedType);
          PacketVector2 position = movement.Position;
          updatedMovement = movement with
          {
            Position = new PacketVector2(
              position.X + (mounted.MountUsesSizeOverride
                ? (current.Width - mounted.Width) * 0.5f : 0f),
              position.Y + current.Height - mounted.Height)
          };
          updated = mounted;
        }
      }
    }
    else if (!mountCommand && current.MountActive)
    {
      if (world?.CanFitDismountedPlayer is null)
      {
        unavailable = true;
        return false;
      }
      PacketVector2 position = world.CanFitDismountedPlayer.Value
        ? world.DismountedPlayerPosition
        : new PacketVector2(movement.Position.X,
          movement.Position.Y + current.Height - 42);
      updatedMovement = movement with { Position = position };
      updated = CreateDismountedState(current);
    }

    updated = updated with
    {
      Pulley = (packet.MovementFlags & 0b0000_0001) != 0,
      VoidVaultEnabled = (packet.PlayerFeatureFlags & 0b0000_0010) != 0
    };
    return true;
  }

  public static int GetMountHeightBoost(ushort mountType)
  {
    return mountType switch
    {
      0 or 1 or 2 or 3 => 20,
      4 => 26,
      5 => 16,
      6 or 15 or >= 18 and <= 22 or >= 24 and <= 36 or 38 or 39 or 51 or 53 => 10,
      7 or 8 or 9 or 17 => 16,
      10 or 40 or 41 or 42 or 47 => 34,
      11 => 12,
      12 or 48 => 14,
      13 or 16 => 10,
      14 or 49 => 8,
      23 or 46 or 52 or 54 or 55 or 56 or >= 57 and <= 61 => 0,
      37 or 43 => 12,
      44 => 24,
      45 => 25,
      50 => 20,
      62 or 63 => 4,
      _ => 0
    };
  }

  private static PlayerRuntimeSpatialState CreateMountedState(
    PlayerRuntimeSpatialState current,
    ushort mountType)
  {
    int heightBoost = GetMountHeightBoost(mountType);
    bool overrideSize = mountType is 55 or 56 or 61;
    (int width, int height) = mountType switch
    {
      55 => (14, 14),
      56 => (20, 18),
      61 => (8, 14),
      _ => (20, 42 + heightBoost)
    };
    return current with
    {
      Width = width,
      Height = height,
      MountActive = true,
      ActiveMountType = mountType,
      MountHeightBoost = heightBoost,
      MountUsesSizeOverride = overrideSize,
      PortableStoolInUse = false
    };
  }

  private static PlayerRuntimeSpatialState CreateDismountedState(
    PlayerRuntimeSpatialState current)
  {
    return current with
    {
      Width = 20,
      Height = 42,
      MountActive = false,
      ActiveMountType = null,
      MountHeightBoost = 0,
      MountUsesSizeOverride = false,
      PortableStoolInUse = false
    };
  }
}
