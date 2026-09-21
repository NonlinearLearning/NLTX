using System.Collections.Generic;

namespace Terraria.WorldStorage;

public interface ILiquidWorkQueueCommitPort
{
  LiquidWorkQueueOperationResult Enqueue(
    in LiquidCellWorkItemStateCommand command);

  LiquidWorkQueueOperationResult Requeue(
    in LiquidCellWorkItemStateCommand command);

  LiquidWorkQueueOperationResult Remove(TileCoordinate coordinate);

  LiquidWorkQueueOperationResult Reset();

  IReadOnlyList<LiquidCellWorkItemStateCommand> Snapshot();
}
