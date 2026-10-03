using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class WorldLoadApiDescriptor
{
  public WorldLoadApiDescriptor(
    string apiId,
    string ownerId,
    string sectionId,
    int minimumFormatVersion,
    int maximumFormatVersion,
    WorldLoadSectionRequirement requirement,
    params string[] commitAfter)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(apiId);
    ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
    ArgumentException.ThrowIfNullOrWhiteSpace(sectionId);
    ArgumentNullException.ThrowIfNull(commitAfter);
    if (minimumFormatVersion < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(minimumFormatVersion));
    }

    if (maximumFormatVersion < minimumFormatVersion)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumFormatVersion));
    }

    if (requirement is not WorldLoadSectionRequirement.Required and
        not WorldLoadSectionRequirement.Optional)
    {
      throw new ArgumentOutOfRangeException(nameof(requirement));
    }

    var dependencyIds = new HashSet<string>(StringComparer.Ordinal);
    foreach (string dependencyId in commitAfter)
    {
      if (string.IsNullOrWhiteSpace(dependencyId) ||
          string.Equals(dependencyId, apiId, StringComparison.Ordinal) ||
          !dependencyIds.Add(dependencyId))
      {
        throw new ArgumentException(
          "CommitAfter must contain distinct, non-empty ApiIds and cannot reference itself.",
          nameof(commitAfter));
      }
    }

    ApiId = apiId;
    OwnerId = ownerId;
    SectionId = sectionId;
    MinimumFormatVersion = minimumFormatVersion;
    MaximumFormatVersion = maximumFormatVersion;
    Requirement = requirement;
    CommitAfter = Array.AsReadOnly((string[])commitAfter.Clone());
  }

  public string ApiId { get; }

  public string OwnerId { get; }

  public string SectionId { get; }

  public int MinimumFormatVersion { get; }

  public int MaximumFormatVersion { get; }

  public WorldLoadSectionRequirement Requirement { get; }

  public IReadOnlyList<string> CommitAfter { get; }
}
