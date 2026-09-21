using System.Collections.ObjectModel;

namespace Terraria.Presentation.BuffText;

public sealed class ProjectileBuffTextFilter
{
  private readonly int[] _projectileTypeIds;
  private readonly ReadOnlyCollection<int> _readOnlyProjectileTypeIds;

  public ProjectileBuffTextFilter(IReadOnlyList<int> projectileTypeIds)
  {
    ArgumentNullException.ThrowIfNull(projectileTypeIds);
    _projectileTypeIds = projectileTypeIds.Distinct().OrderBy(value => value).ToArray();
    _readOnlyProjectileTypeIds = Array.AsReadOnly(_projectileTypeIds);
  }

  public IReadOnlyList<int> ProjectileTypeIds => _readOnlyProjectileTypeIds;

  public int CountActive(IReadOnlyList<int> activeProjectileTypeIds)
  {
    ArgumentNullException.ThrowIfNull(activeProjectileTypeIds);
    int count = 0;
    for (int index = 0; index < activeProjectileTypeIds.Count; index++)
    {
      if (IsTracked(activeProjectileTypeIds[index]))
      {
        count++;
      }
    }

    return count;
  }

  public bool IsTracked(int projectileTypeId)
  {
    return Array.BinarySearch(_projectileTypeIds, projectileTypeId) >= 0;
  }
}
