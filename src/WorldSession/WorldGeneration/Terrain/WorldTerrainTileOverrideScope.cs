using System;

namespace Terraria.WorldGeneration.Terrain;

public sealed class WorldTerrainTileOverrideScope : IDisposable
{
  private readonly bool[] _target;
  private readonly bool[] _backup;
  private bool _restored;

  public WorldTerrainTileOverrideScope(
    bool[] target,
    ReadOnlySpan<bool> backup)
  {
    ArgumentNullException.ThrowIfNull(target);
    if (target.Length != backup.Length)
    {
      throw new ArgumentException(
        "The tile-solid backup must match the target length.",
        nameof(backup));
    }

    _target = target;
    _backup = backup.ToArray();
  }

  public bool IsRestored => _restored;

  public void Restore()
  {
    if (_restored)
    {
      return;
    }

    _backup.AsSpan().CopyTo(_target);
    _restored = true;
  }

  public void Dispose()
  {
    Restore();
  }
}
