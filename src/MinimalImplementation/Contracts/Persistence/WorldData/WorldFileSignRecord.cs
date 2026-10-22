using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A persisted sign text and its tile coordinate.
/// </summary>
public sealed class WorldFileSignRecord
{
  public WorldFileSignRecord(string text, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(text);
    Text = text;
    X = x;
    Y = y;
  }

  public string Text { get; }

  public int X { get; }

  public int Y { get; }
}
