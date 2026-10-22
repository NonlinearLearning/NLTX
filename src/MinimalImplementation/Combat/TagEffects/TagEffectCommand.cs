namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Detached semantic form of one Version4 module 12 message.</summary>
public sealed class TagEffectCommand
{
  public TagEffectCommand(
    byte player,
    byte action,
    short? effectType,
    IReadOnlyCollection<int>? npcTimes,
    IReadOnlyCollection<int>? procTimes,
    byte? npcIndex)
  {
    Player = player;
    Action = action;
    EffectType = effectType;
    NpcTimes = npcTimes is null ? null : Array.AsReadOnly(npcTimes.ToArray());
    ProcTimes = procTimes is null ? null : Array.AsReadOnly(procTimes.ToArray());
    NpcIndex = npcIndex;
  }

  public byte Player { get; }

  public byte Action { get; }

  public short? EffectType { get; }

  public IReadOnlyList<int>? NpcTimes { get; }

  public IReadOnlyList<int>? ProcTimes { get; }

  public byte? NpcIndex { get; }
}
