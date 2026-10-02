using System;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public readonly record struct WiringPropagationCommand
{
  private WiringPropagationCommand(
    WiringPropagationCommandKind kind,
    TileCoordinate coordinate,
    int left,
    int top,
    int width,
    int height,
    int currentUser,
    byte value)
  {
    Kind = kind;
    Coordinate = coordinate;
    Left = left;
    Top = top;
    Width = width;
    Height = height;
    CurrentUser = currentUser;
    Value = value;
  }

  public WiringPropagationCommandKind Kind { get; }

  public TileCoordinate Coordinate { get; }

  public int Left { get; }

  public int Top { get; }

  public int Width { get; }

  public int Height { get; }

  public int CurrentUser { get; }

  public byte Value { get; }

  public static WiringPropagationCommand Initialize()
  {
    return Create(WiringPropagationCommandKind.Initialize);
  }

  public static WiringPropagationCommand BeginTrip(
    int left,
    int top,
    int width,
    int height,
    int currentUser = -1)
  {
    return new WiringPropagationCommand(
      WiringPropagationCommandKind.BeginTrip,
      default,
      left,
      top,
      width,
      height,
      currentUser,
      0);
  }

  public static WiringPropagationCommand BeginWireColorPass(
    byte wireColor)
  {
    return new WiringPropagationCommand(
      WiringPropagationCommandKind.BeginWireColorPass,
      default,
      0,
      0,
      0,
      0,
      0,
      wireColor);
  }

  public static WiringPropagationCommand SkipTile(int x, int y)
  {
    return CreateCoordinateCommand(
      WiringPropagationCommandKind.SkipTile,
      x,
      y);
  }

  public static WiringPropagationCommand QueueTile(
    int x,
    int y,
    byte direction)
  {
    return CreateCoordinateCommand(
      WiringPropagationCommandKind.QueueTile,
      x,
      y,
      direction);
  }

  public static WiringPropagationCommand QueueLamp(int x, int y)
  {
    return CreateCoordinateCommand(
      WiringPropagationCommandKind.QueueLamp,
      x,
      y);
  }

  public static WiringPropagationCommand QueueGate(int x, int y)
  {
    return CreateCoordinateCommand(
      WiringPropagationCommandKind.QueueGate,
      x,
      y);
  }

  public static WiringPropagationCommand QueueNextGate(int x, int y)
  {
    return CreateCoordinateCommand(
      WiringPropagationCommandKind.QueueNextGate,
      x,
      y);
  }

  public static WiringPropagationCommand CompleteGate(int x, int y)
  {
    return CreateCoordinateCommand(
      WiringPropagationCommandKind.CompleteGate,
      x,
      y);
  }

  public static WiringPropagationCommand RecordPixelBoxTrigger(
    int x,
    int y,
    byte trigger)
  {
    return CreateCoordinateCommand(
      WiringPropagationCommandKind.RecordPixelBoxTrigger,
      x,
      y,
      trigger);
  }

  public static WiringPropagationCommand EndTrip()
  {
    return Create(WiringPropagationCommandKind.EndTrip);
  }

  public static WiringPropagationCommand Reset()
  {
    return Create(WiringPropagationCommandKind.Reset);
  }

  private static WiringPropagationCommand Create(
    WiringPropagationCommandKind kind)
  {
    return new WiringPropagationCommand(
      kind,
      default,
      0,
      0,
      0,
      0,
      0,
      0);
  }

  private static WiringPropagationCommand CreateCoordinateCommand(
    WiringPropagationCommandKind kind,
    int x,
    int y,
    byte value = 0)
  {
    if (x < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (y < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(y));
    }

    return new WiringPropagationCommand(
      kind,
      new TileCoordinate(x, y),
      0,
      0,
      0,
      0,
      0,
      value);
  }
}
