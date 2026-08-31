using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcActivityRangeDefinition
{
  public static NpcActivityRangeDefinition Version1456 { get; } = new(
    activeRangeX: 4032,
    activeRangeY: 2520,
    screenWidth: 1920,
    screenHeight: 1200);

  public NpcActivityRangeDefinition(
    int activeRangeX,
    int activeRangeY,
    int screenWidth,
    int screenHeight)
  {
    if (activeRangeX <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(activeRangeX));
    }

    if (activeRangeY <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(activeRangeY));
    }

    if (screenWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(screenWidth));
    }

    if (screenHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(screenHeight));
    }

    ActiveRangeX = activeRangeX;
    ActiveRangeY = activeRangeY;
    ScreenWidth = screenWidth;
    ScreenHeight = screenHeight;
  }

  public int ActiveRangeX { get; }

  public int ActiveRangeY { get; }

  public int ScreenWidth { get; }

  public int ScreenHeight { get; }

  internal void Validate()
  {
    if (ActiveRangeX <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ActiveRangeX));
    }

    if (ActiveRangeY <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ActiveRangeY));
    }

    if (ScreenWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ScreenWidth));
    }

    if (ScreenHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ScreenHeight));
    }
  }
}
