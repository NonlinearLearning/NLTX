using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct TreePlacementComponent
{
  public TreePlacementComponent(string definitionId, int originX, int originY)
    : this()
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
    DefinitionId = definitionId;
    OriginX = originX;
    OriginY = originY;
  }

  public string DefinitionId { get; }

  public int OriginX { get; }

  public int OriginY { get; }
}
