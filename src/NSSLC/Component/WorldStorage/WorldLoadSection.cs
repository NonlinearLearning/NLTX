using System;

namespace Terraria.WorldStorage;

public readonly struct WorldLoadSection<TSection>
  where TSection : notnull
{
  private readonly TSection _value;

  private WorldLoadSection(TSection value, bool isPresent)
  {
    _value = value;
    IsPresent = isPresent;
  }

  public bool IsPresent { get; }

  public TSection Value => IsPresent
    ? _value
    : throw new InvalidOperationException("The optional section is absent.");

  public static WorldLoadSection<TSection> Absent => new(default!, isPresent: false);

  public static WorldLoadSection<TSection> Present(TSection value)
  {
    ArgumentNullException.ThrowIfNull(value);
    return new WorldLoadSection<TSection>(value, isPresent: true);
  }
}
