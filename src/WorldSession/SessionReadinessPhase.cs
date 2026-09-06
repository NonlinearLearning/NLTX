namespace Terraria.WorldSession.Components;

public enum SessionReadinessPhase : byte
{
  AwaitingData,
  ProcessingData,
  Ready,
  Failed,
  Unloading,
}
