using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Queries;

public static class FrameActivityQuery
{
  public static FrameActivitySnapshot Read(FrameActivityStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return new FrameActivitySnapshot(
      state.ActivePlayerCount,
      state.SleepingPlayerCount,
      state.AnyActiveBoss,
      state.HadActiveInteractableProjectile);
  }
}
