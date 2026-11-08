using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A single persisted inventory slot from a world chest.
/// </summary>
public sealed class WorldFileChestItem
{
  public WorldFileChestItem(int stack, int type, byte prefix)
  {
    if (stack < short.MinValue || stack > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }

    if (type < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(type));
    }

    Stack = stack;
    Type = type;
    Prefix = prefix;
  }

  public int Stack { get; }

  public int Type { get; }

  public byte Prefix { get; }

  public bool IsEmpty => Stack == 0;
}
