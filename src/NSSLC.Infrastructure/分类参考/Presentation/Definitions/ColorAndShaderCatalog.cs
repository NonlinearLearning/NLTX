namespace Terraria.NonAuthoritative.ContentDefinitions;

public sealed class ColorAndShaderCatalog
{
  private readonly List<ColorValue> _waterfallColors;
  private readonly List<ColorValue> _liquidColors;

  internal ColorAndShaderCatalog(
    IReadOnlyList<ColorValue> waterfallColors,
    IReadOnlyList<ColorValue> liquidColors)
  {
    _waterfallColors = waterfallColors.ToList();
    _liquidColors = liquidColors.ToList();
  }

  public ColorValue RarityAmber { get; internal set; }

  public ColorValue RarityBlue { get; internal set; }

  public ColorValue RarityPurple { get; internal set; }

  public IReadOnlyList<ColorValue> WaterfallColors => _waterfallColors;

  public IReadOnlyList<ColorValue> LiquidColors => _liquidColors;

  public ColorValue InventoryDefaultColor { get; internal set; }

  public ColorValue InventoryDefaultColorWithOpacity { get; internal set; }

  public int TeamDyeShaderIndex { get; internal set; } = -1;

  public int ColorOnlyShaderIndex { get; internal set; } = -1;
}

public static class ColorCatalogLoadSystem
{
  public static ColorAndShaderCatalog CreateDefault()
  {
    ColorValue[] waterfallColors =
    {
      new(9, 61, 191), new(253, 32, 3), new(143, 143, 143),
      new(59, 29, 131), new(7, 145, 142), new(171, 11, 209),
      new(9, 137, 191), new(168, 106, 32), new(36, 60, 148),
      new(65, 59, 101), new(200, 0, 0), new(0, 0, 0, 0),
      new(0, 0, 0, 0), new(177, 54, 79), new(255, 156, 12),
      new(91, 34, 104), new(102, 104, 34), new(34, 43, 104),
      new(34, 104, 38), new(104, 34, 34), new(76, 79, 102),
      new(104, 61, 34)
    };
    ColorValue[] liquidColors =
    {
      waterfallColors[0], waterfallColors[1], waterfallColors[3], waterfallColors[4],
      waterfallColors[5], waterfallColors[6], waterfallColors[7], waterfallColors[8],
      waterfallColors[9], waterfallColors[10], waterfallColors[13], waterfallColors[14]
    };
    return new ColorAndShaderCatalog(waterfallColors, liquidColors)
    {
      RarityAmber = new ColorValue(255, 175, 0),
      RarityBlue = new ColorValue(150, 150, 255),
      RarityPurple = new ColorValue(210, 160, 255),
      InventoryDefaultColor = new ColorValue(63, 65, 151),
      InventoryDefaultColorWithOpacity = new ColorValue(63, 65, 151, 200)
    };
  }
}

public static class LiquidColorQuery
{
  public static ColorValue CurrentLiquidColor(
    ColorAndShaderCatalog catalog,
    IReadOnlyList<float> liquidAlpha)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(liquidAlpha);
    ColorValue color = new(0, 0, 0, 0);
    bool hasColor = false;
    int count = Math.Min(11, Math.Min(catalog.LiquidColors.Count, liquidAlpha.Count));
    for (int index = 0; index < count; index++)
    {
      float alpha = liquidAlpha[index];
      if (float.IsNaN(alpha) || float.IsInfinity(alpha) || alpha <= 0f)
      {
        continue;
      }

      if (!hasColor)
      {
        color = catalog.LiquidColors[index];
        hasColor = true;
      }
      else
      {
        color = ColorValue.Lerp(color, catalog.LiquidColors[index], Math.Clamp(alpha, 0f, 1f));
      }
    }

    return color;
  }
}

public interface IShaderIndexAdapter
{
  bool TryResolve(string key, out int shaderIndex);
}

public sealed class ShaderIndexAdapter : IShaderIndexAdapter
{
  private readonly Dictionary<string, int> _indexes = new(StringComparer.Ordinal);

  public void Set(string key, int shaderIndex)
  {
    if (string.IsNullOrWhiteSpace(key) || shaderIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(key));
    }

    _indexes[key] = shaderIndex;
  }

  public bool TryResolve(string key, out int shaderIndex)
  {
    return _indexes.TryGetValue(key, out shaderIndex);
  }
}

public static class ShaderIndexProjection
{
  public static void Apply(ColorAndShaderCatalog catalog, IShaderIndexAdapter adapter)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(adapter);
    if (adapter.TryResolve("team", out int team))
    {
      catalog.TeamDyeShaderIndex = team;
    }

    if (adapter.TryResolve("color-only", out int colorOnly))
    {
      catalog.ColorOnlyShaderIndex = colorOnly;
    }
  }
}
