using System;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Required migrated protocol facts for the variable-width fields in packet 23.</summary>
public sealed class NpcPacket23ProtocolFacts
{
  private readonly bool[] _catchableTypes;

  public NpcPacket23ProtocolFacts(
    ReadOnlySpan<bool> catchableTypes,
    Func<short, byte?, float?, byte> lifeWidthResolver)
  {
    ArgumentNullException.ThrowIfNull(lifeWidthResolver);
    if (catchableTypes.IsEmpty)
    {
      throw new ArgumentException("Packet 23 requires an explicit catchable-type table.", nameof(catchableTypes));
    }

    _catchableTypes = catchableTypes.ToArray();
    LifeWidthResolver = lifeWidthResolver;
  }

  public Func<short, byte?, float?, byte> LifeWidthResolver { get; }

  internal bool[] CatchableTypes => _catchableTypes;

  public bool[] CopyCatchableTypeTable()
  {
    return (bool[])_catchableTypes.Clone();
  }
}
