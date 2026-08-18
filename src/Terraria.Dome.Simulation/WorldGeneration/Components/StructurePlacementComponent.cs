using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct StructurePlacementComponent
{
  public StructurePlacementComponent(
    string definitionId,
    int originX,
    int originY,
    StructureFootprintComponent footprint)
    : this()
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
    DefinitionId = definitionId;
    OriginX = originX;
    OriginY = originY;
    Footprint = footprint;
  }

  public string DefinitionId { get; }

  public int OriginX { get; }

  public int OriginY { get; }

  public StructureFootprintComponent Footprint { get; }
}
