using System;
using Terraria.Npc.Queries;

namespace Terraria.Npc;

public static class NpcSpawnEntityPreparationSystem
{
  private const int SlimeNetId = 1;
  private const int VariantLuckRange = 180;
  private const int GoldSlimeType = -4;
  private const int AnniversarySlimeType = 667;

  public static NpcSpawnEntityPreparationResult Prepare(
    in NpcSpawnEntityRequest request,
    bool isAnniversaryWorld,
    in NpcSpawnTargetSelectionSnapshot targetSnapshot,
    INpcSpawnEntityPreparationPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    NpcTypeId fromNetIdType = port.FromNetId(request.Type);
    NpcTypeId preparedType = request.Type;
    bool commonVariantRollConsumed = false;
    bool anniversaryVariantRollConsumed = false;
    if (fromNetIdType.Value == SlimeNetId)
    {
      commonVariantRollConsumed = true;
      if (port.RollLuck(VariantLuckRange) == 0)
      {
        preparedType = new NpcTypeId(GoldSlimeType);
      }

      if (isAnniversaryWorld)
      {
        anniversaryVariantRollConsumed = true;
        if (port.RollLuck(VariantLuckRange) == 0)
        {
          preparedType = new NpcTypeId(AnniversarySlimeType);
        }
      }
    }

    int preparedTarget = NpcSpawnTargetSelectionQuery.Select(
      request.Target,
      in targetSnapshot);
    NpcSpawnEntityRequest preparedRequest = request with
    {
      Type = preparedType,
      Target = preparedTarget,
    };
    return new NpcSpawnEntityPreparationResult(
      request,
      fromNetIdType,
      preparedRequest,
      commonVariantRollConsumed,
      anniversaryVariantRollConsumed);
  }
}
