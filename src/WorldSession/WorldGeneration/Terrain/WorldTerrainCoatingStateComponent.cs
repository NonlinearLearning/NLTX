using System;
using System.Collections.Generic;
using Terraria.Content;

namespace Terraria.WorldGeneration.Terrain;

/// <summary>
/// Owns the current coating colors without exposing mutable storage.
/// </summary>
public sealed class WorldTerrainCoatingStateComponent
{
  private ColorRgba[] _colors = Array.Empty<ColorRgba>();
  private IReadOnlyList<ColorRgba> _colorsView = Array.Empty<ColorRgba>();

  public int Count => _colors.Length;

  public IReadOnlyList<ColorRgba> Colors => _colorsView;

  public void Replace(IReadOnlyList<ColorRgba> colors)
  {
    ArgumentNullException.ThrowIfNull(colors);

    ColorRgba[] copy = new ColorRgba[colors.Count];
    for (int index = 0; index < colors.Count; index++)
    {
      copy[index] = colors[index];
    }

    _colors = copy;
    _colorsView = Array.AsReadOnly(copy);
  }

  public void Clear()
  {
    _colors = Array.Empty<ColorRgba>();
    _colorsView = Array.Empty<ColorRgba>();
  }
}
