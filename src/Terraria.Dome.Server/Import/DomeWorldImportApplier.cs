using System;
using System.IO;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.WorldCompatibility.Projection;
using Terraria.WorldFile.V319;
using Terraria.WorldFile.V319.Model;

namespace Terraria.Dome.Server.Import;

public sealed class DomeWorldImportApplier
{
  public DomeWorldImportResult Import(string worldPath, bool strictImport = true)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(worldPath);
    if (!Path.IsPathFullyQualified(worldPath))
    {
      throw new ArgumentException("The world path must be absolute.", nameof(worldPath));
    }

    using FileStream input = new(
      worldPath,
      FileMode.Open,
      FileAccess.Read,
      FileShare.Read);
    LegacyWorldDocument document = WldWorldReader.Read(input);
    Terraria.WorldCompatibility.Model.CompatibilityWorldSnapshot compatibility =
      WldToCompatibilityProjection.Project(document);
    DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
      compatibility,
      new WorldSeed(document.Metadata.WorldId),
      strictImport);
    return new DomeWorldImportResult(document, compatibility, snapshot);
  }
}
