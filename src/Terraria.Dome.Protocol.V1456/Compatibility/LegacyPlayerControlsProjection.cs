using Terraria.Dome.Protocol.V1456.Packets;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public sealed record LegacyPlayerControlsProjection(
  PlayerControlIntent Intent,
  LegacyPlayerControlsState State)
{
  public static LegacyPlayerControlsProjection Create(LegacyPlayerControlsState state)
  {
    return new LegacyPlayerControlsProjection(
      new PlayerControlIntent(
        state.PlayerSlot,
        (state.ControlFlags & (1 << 2)) != 0,
        (state.ControlFlags & (1 << 3)) != 0,
        (state.ControlFlags & (1 << 4)) != 0,
        (state.ControlFlags & (1 << 5)) != 0,
        (state.ControlFlags & (1 << 6)) != 0,
        state.SelectedItem),
      state);
  }
}
