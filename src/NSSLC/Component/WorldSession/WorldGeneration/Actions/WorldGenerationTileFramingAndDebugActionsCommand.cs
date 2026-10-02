using System;

using Terraria.Content;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

public readonly record struct WorldGenerationTileFramingAndDebugActionsCommand
{
  public enum OperationKind : byte
  {
    SetFrames,
    DebugDraw,
  }

  public interface IDiagnosticSink
  {
    void Publish(TilePosition target, ColorRgba color);
  }

  private WorldGenerationTileFramingAndDebugActionsCommand(
    OperationKind kind,
    TilePosition target,
    bool frameNeighbors,
    ColorRgba diagnosticColor,
    IDiagnosticSink? diagnosticSink)
  {
    Kind = kind;
    Target = target;
    FrameNeighbors = frameNeighbors;
    DiagnosticColor = diagnosticColor;
    DiagnosticSink = diagnosticSink;
  }

  public OperationKind Kind { get; }

  public TilePosition Target { get; }

  public bool FrameNeighbors { get; }

  public ColorRgba DiagnosticColor { get; }

  public IDiagnosticSink? DiagnosticSink { get; }

  public static WorldGenerationTileFramingAndDebugActionsCommand SetFrames(
    TilePosition target,
    bool frameNeighbors = false)
  {
    return new WorldGenerationTileFramingAndDebugActionsCommand(
      OperationKind.SetFrames,
      target,
      frameNeighbors,
      default,
      null);
  }

  public static WorldGenerationTileFramingAndDebugActionsCommand DebugDraw(
    TilePosition target,
    ColorRgba diagnosticColor,
    IDiagnosticSink diagnosticSink)
  {
    ArgumentNullException.ThrowIfNull(diagnosticSink);
    return new WorldGenerationTileFramingAndDebugActionsCommand(
      OperationKind.DebugDraw,
      target,
      false,
      diagnosticColor,
      diagnosticSink);
  }

  public bool IsWellFormed =>
    Kind switch
    {
      OperationKind.SetFrames =>
        DiagnosticSink is null && DiagnosticColor == default,
      OperationKind.DebugDraw =>
        DiagnosticSink is not null && !FrameNeighbors,
      _ => false,
    };

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "The framing or diagnostic command contains an unsupported operation or payload.",
        nameof(Kind));
    }
  }
}
