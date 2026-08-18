using System.Collections.Generic;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct TagEffectModulePacket(
  byte OwnerSlot,
  TagEffectMessageType MessageType,
  short? EffectType,
  IReadOnlyList<TagEffectSparseEntry>? TaggedNpcTimes,
  IReadOnlyList<TagEffectSparseEntry>? ProcNpcTimes,
  byte? NpcIndex);
