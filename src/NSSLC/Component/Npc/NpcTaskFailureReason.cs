namespace Terraria.Npc;

public enum NpcTaskFailureReason : byte
{
  None,
  TargetUnavailable,
  NoPath,
  CapacityRejected,
  CapabilityRejected,
  TaskReplaced,
  Aborted,
  Exception,
}
