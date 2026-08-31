using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcActivityRangePolicy
{
  public static NpcActivityRangeDecision Evaluate(
    NpcPixelBounds npc,
    NpcActivityRangeDefinition range,
    IReadOnlyList<NpcPlayerActivity> players)
  {
    npc.Validate();
    range.Validate();
    ArgumentNullException.ThrowIfNull(players);

    NpcPixelRectangle activeRange = CreateActiveRange(npc, range);
    NpcPixelRectangle screenRange = CreateScreenRange(npc, range);
    bool hasActivePlayerInRange = false;
    bool shouldRefreshInactivityTimer = false;

    for (int index = 0; index < players.Count; index++)
    {
      NpcPlayerActivity player = players[index];
      if (!player.IsActive)
      {
        continue;
      }

      player.Hitbox.Validate();
      if (!hasActivePlayerInRange && activeRange.Intersects(player.Hitbox))
      {
        hasActivePlayerInRange = true;
      }

      if (!shouldRefreshInactivityTimer && screenRange.Intersects(player.Hitbox))
      {
        shouldRefreshInactivityTimer = true;
      }

      if (hasActivePlayerInRange && shouldRefreshInactivityTimer)
      {
        break;
      }
    }

    return new(hasActivePlayerInRange, shouldRefreshInactivityTimer);
  }

  private static NpcPixelRectangle CreateActiveRange(
    NpcPixelBounds npc,
    NpcActivityRangeDefinition range)
  {
    float left = npc.X + (float)(npc.Width / 2) - range.ActiveRangeX;
    float top = npc.Y + (float)(npc.Height / 2) - range.ActiveRangeY;
    int pixelWidth = ToPixelDimension(2L * range.ActiveRangeX, nameof(range.ActiveRangeX));
    int pixelHeight = ToPixelDimension(2L * range.ActiveRangeY, nameof(range.ActiveRangeY));
    return new(
      ToPixelCoordinate(left, nameof(npc)),
      ToPixelCoordinate(top, nameof(npc)),
      pixelWidth,
      pixelHeight);
  }

  private static NpcPixelRectangle CreateScreenRange(
    NpcPixelBounds npc,
    NpcActivityRangeDefinition range)
  {
    double centerX = (double)(npc.X + (float)(npc.Width / 2));
    double centerY = (double)(npc.Y + (float)(npc.Height / 2));
    double left = centerX - range.ScreenWidth * 0.5d - npc.Width;
    double top = centerY - range.ScreenHeight * 0.5d - npc.Height;
    long pixelWidth = (long)range.ScreenWidth + 2L * npc.Width;
    long pixelHeight = (long)range.ScreenHeight + 2L * npc.Height;
    return new(
      ToPixelCoordinate(left, nameof(npc)),
      ToPixelCoordinate(top, nameof(npc)),
      ToPixelDimension(pixelWidth, nameof(range.ScreenWidth)),
      ToPixelDimension(pixelHeight, nameof(range.ScreenHeight)));
  }

  private static int ToPixelCoordinate(double value, string parameterName)
  {
    if (!double.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    double truncated = Math.Truncate(value);
    if (truncated < int.MinValue || truncated > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return (int)truncated;
  }

  private static int ToPixelDimension(long value, string parameterName)
  {
    if (value <= 0 || value > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return (int)value;
  }
}
