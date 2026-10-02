namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class TownNpcSpawnEligibilityProjection
{
  private bool[] _canSpawn = Array.Empty<bool>();

  public IReadOnlyList<bool> Values => _canSpawn;

  public void Replace(IReadOnlyList<bool> values)
  {
    ArgumentNullException.ThrowIfNull(values);
    var copy = new bool[values.Count];
    for (var index = 0; index < values.Count; index++)
    {
      copy[index] = values[index];
    }

    _canSpawn = copy;
  }

  public bool CanSpawn(int npcType)
  {
    return npcType >= 0 && npcType < _canSpawn.Length && _canSpawn[npcType];
  }
}
