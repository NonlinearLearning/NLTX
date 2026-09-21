using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Session;

public sealed class SessionReadinessSystem
{
  public void BeginLoading(SessionReadinessComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.Phase = SessionReadinessPhase.Loading;
    state.FailureCode = null;
    state.GenerationBarrierActive = true;
  }

  public bool TryMarkReady(
    SessionReadinessComponent state,
    bool loadedEverything,
    bool generationActive)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.GenerationBarrierActive = generationActive;
    if (!loadedEverything || generationActive || state.Phase == SessionReadinessPhase.Failed)
    {
      return false;
    }

    state.Phase = SessionReadinessPhase.Ready;
    return true;
  }

  public void Fail(SessionReadinessComponent state, string failureCode)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (string.IsNullOrWhiteSpace(failureCode))
    {
      throw new ArgumentException("A failure code is required.", nameof(failureCode));
    }

    state.Phase = SessionReadinessPhase.Failed;
    state.FailureCode = failureCode;
    state.GenerationBarrierActive = false;
  }

  public void Unload(SessionReadinessComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.Phase = SessionReadinessPhase.Unloaded;
    state.InMenu = true;
    state.FailureCode = null;
    state.GenerationBarrierActive = false;
  }

  public void SetMenuState(SessionReadinessComponent state, bool inMenu)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.InMenu = inMenu;
  }
}
