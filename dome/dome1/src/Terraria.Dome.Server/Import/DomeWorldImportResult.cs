using Terraria.Dome.Simulation;
using Terraria.WorldCompatibility.Model;
using Terraria.WorldFile.V319.Model;

namespace Terraria.Dome.Server.Import;

public sealed class DomeWorldImportResult
{
  public DomeWorldImportResult(
    LegacyWorldDocument document,
    CompatibilityWorldSnapshot compatibility,
    DomeSimulationSnapshot snapshot)
  {
    Document = document;
    Compatibility = compatibility;
    Snapshot = snapshot;
  }

  public CompatibilityWorldSnapshot Compatibility { get; }

  public LegacyWorldDocument Document { get; }

  public DomeSimulationSnapshot Snapshot { get; }
}
