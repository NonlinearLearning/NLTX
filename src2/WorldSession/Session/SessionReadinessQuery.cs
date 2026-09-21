using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Session;

public static class SessionReadinessQuery
{
  public static bool IsEntityUpdateAllowed(SessionReadinessComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.Phase == SessionReadinessPhase.Ready &&
      !state.InMenu &&
      !state.GenerationBarrierActive;
  }
}
