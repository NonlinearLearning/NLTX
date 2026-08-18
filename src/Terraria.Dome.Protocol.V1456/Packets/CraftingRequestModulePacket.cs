using System.Collections.Generic;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct CraftingRequestModulePacket(
  IReadOnlyList<CraftingRequiredItemPacket> Items,
  IReadOnlyList<int> ChestIndices);
