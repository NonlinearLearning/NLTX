using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct SimpleStructurePattern
{
  private readonly int[] _actionIndices;

  private SimpleStructurePattern(
    int width,
    int height,
    int[] actionIndices,
    bool horizontalMirror,
    bool verticalMirror)
  {
    Width = width;
    Height = height;
    _actionIndices = actionIndices;
    HorizontalMirror = horizontalMirror;
    VerticalMirror = verticalMirror;
  }

  public int Width { get; }

  public int Height { get; }

  public bool HorizontalMirror { get; }

  public bool VerticalMirror { get; }

  public static SimpleStructurePattern Parse(IReadOnlyList<string> lines)
  {
    ArgumentNullException.ThrowIfNull(lines);
    if (lines.Count == 0 || string.IsNullOrEmpty(lines[0]))
    {
      throw new ArgumentException("A structure pattern must contain at least one column.", nameof(lines));
    }

    int width = lines[0].Length;
    int height = lines.Count;
    int[] actionIndices = new int[width * height];
    Array.Fill(actionIndices, -1);
    for (int y = 0; y < height; y++)
    {
      string line = lines[y] ?? throw new ArgumentException(
        "A structure pattern cannot contain a null row.", nameof(lines));
      if (line.Length != width)
      {
        throw new ArgumentException("All structure pattern rows must have the same width.", nameof(lines));
      }

      for (int x = 0; x < width; x++)
      {
        char value = line[x];
        actionIndices[(y * width) + x] = value is >= '0' and <= '9' ? value - '0' : -1;
      }
    }

    return new SimpleStructurePattern(width, height, actionIndices, false, false);
  }

  public SimpleStructurePattern Mirror(bool horizontalMirror, bool verticalMirror)
  {
    return new SimpleStructurePattern(
      Width,
      Height,
      _actionIndices,
      horizontalMirror,
      verticalMirror);
  }

  public int GetActionIndex(int x, int y)
  {
    if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
    {
      throw new ArgumentOutOfRangeException();
    }

    return _actionIndices[(y * Width) + x];
  }
}
