using System.Threading;

namespace Terraria.WorldStorage;

public abstract class WorldLoadApiBindings : WorldLoadApiRuntimeBindings
{
  public abstract int FormatVersion { get; }

  public abstract CancellationToken CancellationToken { get; }

  public abstract bool TryGetSection<TSection>(
    string sectionId,
    out WorldLoadSection<TSection> section)
    where TSection : notnull;
}
