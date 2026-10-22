using System;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldPersistenceSection
{
  private readonly object _value;

  private WorldPersistenceSection(string sectionId, Type sectionType, object value)
  {
    SectionId = sectionId;
    SectionType = sectionType;
    _value = value;
  }

  public string SectionId { get; }

  internal Type SectionType { get; }

  public static WorldPersistenceSection Create<TSection>(
    string sectionId,
    TSection value)
    where TSection : notnull
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(sectionId);
    ArgumentNullException.ThrowIfNull(value);
    return new WorldPersistenceSection(sectionId, typeof(TSection), value);
  }

  internal TSection GetValue<TSection>()
    where TSection : notnull
  {
    if (_value is TSection value)
    {
      return value;
    }

    throw new InvalidDataException(
      $"World section '{SectionId}' does not contain the expected type '{typeof(TSection)}'.");
  }
}
