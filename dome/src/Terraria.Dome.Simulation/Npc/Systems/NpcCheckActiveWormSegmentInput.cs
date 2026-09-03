using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveWormSegmentInput(
  NpcHandle Trigger,
  int TriggerAiStyle,
  float TriggerNextSegment,
  IReadOnlyList<NpcCheckActiveWormSegmentState> Segments,
  int MaxNpcCount = NpcCheckActiveWormSegmentPolicy.Version1456MaxNpcCount);
