namespace Terraria.NonAuthoritative.ContentDefinitions;

public sealed record TileStyleDefinition
{
  public TileStyleDefinition(int style, int width, int height, int step)
  {
    if (style < 0 || width <= 0 || height <= 0 || step <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(style));
    }

    Style = style;
    Width = width;
    Height = height;
    Step = step;
  }

  public int Style { get; }

  public int Width { get; }

  public int Height { get; }

  public int Step { get; }
}

public sealed class TileObjectStyleDefinitionAndSelection
{
  private readonly Dictionary<int, TileStyleDefinition> _presets = new();
  private readonly Dictionary<int, int> _overrides = new();

  public bool IsReadOnly { get; internal set; }

  public bool SkipVisualFrame { get; internal set; }

  public IReadOnlyDictionary<int, TileStyleDefinition> Presets => _presets;

  public IReadOnlyDictionary<int, int> Overrides => _overrides;

  internal void Register(TileStyleDefinition definition)
  {
    EnsureWritable();
    _presets[definition.Style] = definition;
  }

  internal void SetOverride(int requestedStyle, int selectedStyle)
  {
    EnsureWritable();
    if (!_presets.ContainsKey(selectedStyle))
    {
      throw new ArgumentOutOfRangeException(nameof(selectedStyle));
    }

    _overrides[requestedStyle] = selectedStyle;
  }

  internal void Freeze()
  {
    IsReadOnly = true;
  }

  internal bool TryGetOverride(int style, out int selectedStyle)
  {
    return _overrides.TryGetValue(style, out selectedStyle);
  }

  private void EnsureWritable()
  {
    if (IsReadOnly)
    {
      throw new InvalidOperationException("Tile styles are read-only after registration.");
    }
  }
}

public readonly record struct TileStyleSelectionInput(
  int RequestedStyle,
  int StyleCount,
  IReadOnlyList<int>? SpecificRandomStyles,
  IStyleRandomSource RandomSource);

public interface IStyleRandomSource
{
  int Next(int exclusiveMax);
}

public readonly record struct TileStyleSelectionResult(int Style, bool UsedOverride);

public static class TileStyleRegistrationSystem
{
  public static void Register(
    TileObjectStyleDefinitionAndSelection catalog,
    TileStyleDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(definition);
    catalog.Register(definition);
  }

  public static void SetOverride(
    TileObjectStyleDefinitionAndSelection catalog,
    int requestedStyle,
    int selectedStyle)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.SetOverride(requestedStyle, selectedStyle);
  }

  public static void Freeze(TileObjectStyleDefinitionAndSelection catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.Freeze();
  }
}

public static class TileStyleSelectionQuery
{
  public static int Select(
    TileObjectStyleDefinitionAndSelection catalog,
    TileStyleSelectionInput input)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(input.RandomSource);
    if (input.StyleCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input.StyleCount));
    }

    if (catalog.TryGetOverride(input.RequestedStyle, out int selectedStyle))
    {
      return selectedStyle;
    }

    IReadOnlyList<int>? candidates = input.SpecificRandomStyles;
    if (candidates is { Count: > 0 })
    {
      return candidates[input.RandomSource.Next(candidates.Count)];
    }

    return input.RandomSource.Next(input.StyleCount);
  }
}
