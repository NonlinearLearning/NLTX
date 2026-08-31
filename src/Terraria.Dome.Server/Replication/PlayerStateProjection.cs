using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Server.Replication;

public static class PlayerStateProjection
{
  public static IReadOnlyList<byte[]> CreateFrames(
    byte playerSlot,
    PlayerSnapshot snapshot,
    PlayerStateSnapshot state)
  {
    if (state.AssignedSlot != playerSlot)
    {
      throw new ArgumentException("Player state slot does not match the projection slot.", nameof(state));
    }

    if (state.SelectedSlot < 0 || state.SelectedSlot >= InventoryComponent.HotbarSlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(state), "Selected slot must be a hotbar slot.");
    }

    List<byte[]> frames = new(4)
    {
      TerrariaPacketCodec.EncodePlayerActive(playerSlot, state.IsActive),
      TerrariaPacketCodec.EncodePlayerLifeMana(
        playerSlot,
        state.Health,
        state.MaximumHealth),
      TerrariaPacketCodec.EncodePlayerControls(
        new PlayerControlIntent(
          playerSlot,
          MoveLeft: state.MoveLeft,
          MoveRight: state.MoveRight,
          Jump: state.Jump,
          UseItem: state.UseItem,
          FacingRight: state.Facing > 0,
          SelectedItem: checked((byte)state.SelectedSlot),
          Down: state.Down),
        TerrariaWorldCoordinates.ToPixels(snapshot.Position.X),
        TerrariaWorldCoordinates.ToPixels(snapshot.Position.Y),
        state.MountType),
      TerrariaPacketCodec.EncodePlayerMana(playerSlot, state.Mana, state.MaximumMana)
    };
    return frames;
  }
}
