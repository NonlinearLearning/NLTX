namespace MinimalImplementation.Combat.Npcs;

/// <summary>Version4 facts used by NPC buff application and packet 137 removal.</summary>
public sealed record NpcBuffDefinition
{
  public NpcBuffDefinition(ushort type, bool isDebuff, bool canBeRemovedByNetwork)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(type);
    Type = type;
    IsDebuff = isDebuff;
    CanBeRemovedByNetwork = canBeRemovedByNetwork;
  }

  public ushort Type { get; }

  public bool IsDebuff { get; }

  public bool CanBeRemovedByNetwork { get; }
}
