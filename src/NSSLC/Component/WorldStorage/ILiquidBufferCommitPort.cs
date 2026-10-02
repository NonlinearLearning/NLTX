using System.Collections.Generic;

namespace Terraria.WorldStorage;

public interface ILiquidBufferCommitPort
{
  LiquidBufferCommitResult Commit(
    in LiquidBufferCommand command,
    bool tileIsCheckingLiquid);

  bool TryPeek(out LiquidBufferEntry entry);

  IReadOnlyList<LiquidBufferEntry> Snapshot();
}
