using System;

namespace Terraria.WorldStorage;

public static class LiquidWorkItemReadyQuery
{
  public static LiquidWorkItemReadiness Evaluate(
    in LiquidCellWorkItemStateCommand command,
    int killThreshold)
  {
    ValidateNonNegative(killThreshold, nameof(killThreshold));

    bool shouldRemove = command.Kill >= killThreshold;
    bool shouldDecrementDelay = !shouldRemove && command.Delay > 0;
    bool isReady = !shouldRemove && command.Delay == 0;
    return new LiquidWorkItemReadiness(
      isReady,
      shouldRemove,
      shouldDecrementDelay);
  }

  public static LiquidCellWorkItemStateCommand DecrementDelay(
    in LiquidCellWorkItemStateCommand command)
  {
    return command.Delay == 0
      ? command
      : command.WithKillAndDelay(command.Kill, command.Delay - 1);
  }

  public static bool IsWithinBounds(
    in LiquidCellWorkItemStateCommand command,
    int worldWidth,
    int worldHeight)
  {
    ValidatePositive(worldWidth, nameof(worldWidth));
    ValidatePositive(worldHeight, nameof(worldHeight));
    return command.X < worldWidth && command.Y < worldHeight;
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  private static void ValidatePositive(int value, string parameterName)
  {
    if (value <= 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
