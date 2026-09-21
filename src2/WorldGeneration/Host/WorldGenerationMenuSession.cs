using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Host;

public sealed class WorldGenerationMenuSession
{
  private readonly float[] _menuItemScales;

  public WorldGenerationMenuSession(int maxMenuItems)
  {
    if (maxMenuItems <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxMenuItems));
    }

    MaxMenuItems = maxMenuItems;
    _menuItemScales = Enumerable.Repeat(0.8f, maxMenuItems).ToArray();
  }

  public int MaxMenuItems { get; }

  public IReadOnlyList<float> MenuItemScales =>
    new ReadOnlyCollection<float>(_menuItemScales.ToArray());

  public int MenuMode { get; private set; }

  public string NewWorldName { get; private set; } = string.Empty;

  public bool AutoPass { get; private set; }

  public void SetMenuMode(int menuMode)
  {
    if (menuMode < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(menuMode));
    }

    MenuMode = menuMode;
  }

  public void SetNewWorldName(string worldName)
  {
    ArgumentNullException.ThrowIfNull(worldName);
    NewWorldName = worldName;
  }

  public void SetAutoPass(bool autoPass)
  {
    AutoPass = autoPass;
  }

  public void SetMenuItemScale(int index, float scale)
  {
    if (index < 0 || index >= _menuItemScales.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    if (!float.IsFinite(scale) || scale < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    _menuItemScales[index] = scale;
  }
}
