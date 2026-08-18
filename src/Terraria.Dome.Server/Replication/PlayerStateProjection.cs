using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public static class PlayerStateProjection
{
  public static IReadOnlyList<byte[]> CreateFrames(
    byte playerSlot,
    PlayerSnapshot snapshot,
    PlayerStateSnapshot state)
  {
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
          MoveLeft: false,
          MoveRight: false,
          Jump: false,
          UseItem: false,
          FacingRight: snapshot.Facing > 0,
          SelectedItem: 0),
        TerrariaWorldCoordinates.ToPixels(snapshot.Position.X),
        TerrariaWorldCoordinates.ToPixels(snapshot.Position.Y)),
      TerrariaPacketCodec.EncodePlayerMana(playerSlot, state.Mana, state.MaximumMana)
    };
    return frames;
  }
}
