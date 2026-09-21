namespace Terraria.SpatialMotionPhysics;

public sealed class ProjectileDefinitionCatalog
{
  private readonly int[] _frames;
  private readonly bool[] _pets;

  internal ProjectileDefinitionCatalog(int count)
  {
    if (count < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    _frames = new int[count];
    _pets = new bool[count];
  }

  public int Count => _frames.Length;

  public void SetFrames(int projectileType, int frames)
  {
    ValidateIndex(projectileType);
    if (frames < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frames));
    }

    _frames[projectileType] = frames;
  }

  public int GetFrames(int projectileType)
  {
    ValidateIndex(projectileType);
    return _frames[projectileType];
  }

  public void SetPet(int projectileType, bool isPet)
  {
    ValidateIndex(projectileType);
    _pets[projectileType] = isPet;
  }

  public bool IsPet(int projectileType)
  {
    ValidateIndex(projectileType);
    return _pets[projectileType];
  }

  private void ValidateIndex(int projectileType)
  {
    if (projectileType < 0 || projectileType >= _frames.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileType));
    }
  }
}
