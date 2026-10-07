using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

public enum LeashedNetworkFrameKind : byte
{
  Remove,
  FullSync,
  PartialSync
}

public readonly record struct LeashedNetworkFrame(
  LeashedNetworkFrameKind Kind,
  int LegacySlot,
  uint SlotGeneration,
  int DefinitionId,
  SectionCoordinate Section);

