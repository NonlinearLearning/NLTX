using System;

namespace Terraria.Dome.Simulation;

public sealed record SignPersistentState(
  int SignId,
  int TileX,
  int TileY,
  string Text,
  long Revision)
{
  public string Text { get; init; } = Text ?? throw new ArgumentNullException(nameof(Text));
}
