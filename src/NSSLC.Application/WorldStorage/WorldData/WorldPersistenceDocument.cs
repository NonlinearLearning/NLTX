using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldPersistenceDocument
{
  private readonly Dictionary<string, WorldPersistenceSection> _sections;
  private readonly IReadOnlyCollection<string> _sectionIds;

  public WorldPersistenceDocument(
    int formatVersion,
    IEnumerable<WorldPersistenceSection> sections)
  {
    if (formatVersion < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(formatVersion));
    }

    ArgumentNullException.ThrowIfNull(sections);

    FormatVersion = formatVersion;
    _sections = new Dictionary<string, WorldPersistenceSection>(StringComparer.Ordinal);
    foreach (WorldPersistenceSection section in sections)
    {
      ArgumentNullException.ThrowIfNull(section);
      if (!_sections.TryAdd(section.SectionId, section))
      {
        throw new ArgumentException(
          $"World section '{section.SectionId}' occurs more than once.",
          nameof(sections));
      }
    }

    _sectionIds = Array.AsReadOnly(
      _sections.Keys.OrderBy(sectionId => sectionId, StringComparer.Ordinal).ToArray());
  }

  public int FormatVersion { get; }

  public IReadOnlyCollection<string> SectionIds => _sectionIds;

  public bool TryGetSection<TSection>(
    string sectionId,
    out WorldLoadSection<TSection> section)
    where TSection : notnull
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(sectionId);
    if (!_sections.TryGetValue(sectionId, out WorldPersistenceSection? storedSection))
    {
      section = WorldLoadSection<TSection>.Absent;
      return false;
    }

    if (storedSection.SectionType != typeof(TSection))
    {
      throw new InvalidDataException(
        $"World section '{sectionId}' has type '{storedSection.SectionType}', " +
        $"not '{typeof(TSection)}'.");
    }

    section = WorldLoadSection<TSection>.Present(storedSection.GetValue<TSection>());
    return true;
  }
}
