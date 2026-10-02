namespace Terraria.WorldSession.Runtime;

public enum DeferredProcessResult
{
  Completed,
  KeepQueued,
  Cancelled,
  Failed
}
