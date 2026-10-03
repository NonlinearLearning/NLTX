using System;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class WorldPersistenceSectionSchemaAttribute : Attribute
{
  public WorldPersistenceSectionSchemaAttribute(
    string sectionId,
    Type sectionType,
    WorldLoadSectionRequirement requirement)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(sectionId);
    ArgumentNullException.ThrowIfNull(sectionType);
    if (requirement is not WorldLoadSectionRequirement.Required and
        not WorldLoadSectionRequirement.Optional)
    {
      throw new ArgumentOutOfRangeException(nameof(requirement));
    }

    SectionId = sectionId;
    SectionType = sectionType;
    Requirement = requirement;
  }

  public string SectionId { get; }

  public Type SectionType { get; }

  public WorldLoadSectionRequirement Requirement { get; }
}
