namespace Terraria.WorldSession.Components;

public enum SessionReadinessPhase
{
  AwaitingData,
  Loading,
  Ready,
  Failed,
  Unloaded
}
