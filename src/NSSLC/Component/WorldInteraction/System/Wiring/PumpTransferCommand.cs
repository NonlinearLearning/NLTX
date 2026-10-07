using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public readonly record struct PumpTransferCommand
{
  private PumpTransferCommand(
    TileCoordinate source,
    TileCoordinate destination,
    int inputIndex,
    int outputIndex,
    byte wireColor)
  {
    Source = source;
    Destination = destination;
    InputIndex = inputIndex;
    OutputIndex = outputIndex;
    WireColor = wireColor;
  }

  public TileCoordinate Source { get; }

  public TileCoordinate Destination { get; }

  public int InputIndex { get; }

  public int OutputIndex { get; }

  public byte WireColor { get; }

  public bool IsValid =>
    Source.X >= 0 && Source.Y >= 0 &&
    Destination.X >= 0 && Destination.Y >= 0 &&
    Source != Destination &&
    InputIndex >= 0 && OutputIndex >= 0 &&
    WireColor is >= 1 and <= 4;

  internal static bool TryCreate(
    TileCoordinate source,
    TileCoordinate destination,
    int inputIndex,
    int outputIndex,
    byte wireColor,
    out PumpTransferCommand command)
  {
    command = default;
    if (source.X < 0 || source.Y < 0 ||
        destination.X < 0 || destination.Y < 0 ||
        source == destination || inputIndex < 0 || outputIndex < 0 ||
        wireColor is < 1 or > 4)
    {
      return false;
    }

    command = new PumpTransferCommand(
      source,
      destination,
      inputIndex,
      outputIndex,
      wireColor);
    return true;
  }
}
