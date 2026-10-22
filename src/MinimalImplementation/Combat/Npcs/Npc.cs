using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Authoritative runtime state for one NPC generation.</summary>
public sealed partial class Npc
{
  private readonly NpcDefinition _definition;
  private int _maximumLife;

  internal Npc(
    CombatEntityIdentity entity,
    short slot,
    NpcDefinition definition,
    NpcStateUpdate initialState,
    NpcHomeState home,
    CommitVersion commitVersion)
  {
    Entity = entity;
    Slot = slot;
    _definition = definition;
    Home = home;
    _combatStats = new NpcCombatStats(
        definition.Damage, definition.Defense, definition.KnockbackResist);
    ApplyState(initialState);
    Revision = 1;
    CommitVersion = commitVersion;
    IsActive = true;
    IsDirty = true;
  }

  public CombatEntityIdentity Entity { get; }

  public short Slot { get; }

  public short Type => _definition.Type;

  internal NpcDefinition Definition => _definition;

  public Vector2 Position { get; private set; }

  public Vector2 Velocity { get; private set; }

  public ushort Target { get; private set; }

  public bool DirectionPositive { get; private set; }

  public bool DirectionYPositive { get; private set; }

  public bool SpriteDirectionPositive { get; private set; }

  public IReadOnlyList<float> Ai { get; private set; } = Array.Empty<float>();

  public int Life { get; private set; }

  public int MaximumLife => _maximumLife;

  public byte? CatchableReleaseOwner { get; private set; }

  public bool SpawnedFromStatue { get; private set; }

  public bool SpawnNeedsSyncing { get; private set; }

  public bool Shimmering { get; private set; }

  public byte? PlayerCount { get; private set; }

  public float? Difficulty { get; private set; }

  public int ExtraValue { get; private set; }

  public Vector2 ExtraValuePosition { get; private set; }

  public bool IsActive { get; private set; }

  public bool IsDirty { get; private set; }

  public long Revision { get; private set; }

  public CommitVersion CommitVersion { get; private set; }

  public bool IsBoss => _definition.IsBoss;

  public bool IsTownNpc => _definition.IsTownNpc;

  public NpcHomeState Home { get; private set; }

  public bool NetAlways => _definition.NetAlways;

  public bool Catchable => _definition.Catchable;

  public NpcAiProfile AiProfile => _definition.AiProfile;

  public int Damage => _combatStats.Damage;

  public int Defense => _combatStats.Defense;

  public bool Friendly => _definition.Friendly;

  public bool DontTakeDamage => _definition.DontTakeDamage;

  public float KnockbackResist => _combatStats.KnockbackResist;

  internal void Apply(NpcStateUpdate update, CommitVersion commitVersion)
  {
    ApplyState(update);
    Revision++;
    CommitVersion = commitVersion;
    IsDirty = true;
  }

  internal void ApplyExtraValue(int extraValue, Vector2 position, CommitVersion commitVersion)
  {
    ExtraValue = extraValue;
    ExtraValuePosition = position;
    Revision++;
    CommitVersion = commitVersion;
    IsDirty = true;
  }

  internal void ApplyHome(NpcHomeState home, CommitVersion commitVersion) {
    Home = home;
    Revision++;
    CommitVersion = commitVersion;
  }

  internal NpcStateUpdate CreateStateUpdate(
    Vector2 position,
    Vector2 velocity,
    ushort target,
    bool directionPositive,
    bool directionYPositive,
    bool spriteDirectionPositive,
    IReadOnlyList<float> ai,
    int life)
  {
    return new NpcStateUpdate(
      position,
      velocity,
      target,
      directionPositive,
      directionYPositive,
      spriteDirectionPositive,
      ai,
      life,
      MaximumLife,
      SpawnedFromStatue,
      SpawnNeedsSyncing,
      Shimmering,
      PlayerCount,
      Difficulty,
      CatchableReleaseOwner);
  }

  internal void Retire()
  {
    IsActive = false;
    IsDirty = false;
    ClearBuffs();
    ClearSlimeBodyItem();
  }

  internal void Acknowledge(long revision)
  {
    if (Revision == revision)
    {
      IsDirty = false;
    }
  }

  internal NpcView Capture()
  {
    return new NpcView(
      Entity,
      Slot,
      Type,
      _definition.AiStyle,
      _definition.Width,
      _definition.Height,
      MaximumLife,
      Position,
      Velocity,
      Target,
      DirectionPositive,
      DirectionYPositive,
      SpriteDirectionPositive,
      Array.AsReadOnly(Ai.ToArray()),
      Life,
      IsBoss,
      IsTownNpc,
      NetAlways,
      Catchable,
      CatchableReleaseOwner,
      SpawnedFromStatue,
      SpawnNeedsSyncing,
      Shimmering,
      PlayerCount,
      Difficulty,
      Revision,
      CommitVersion,
      IsDirty,
      ActivityTimeLeft,
      Damage,
      Defense,
      KnockbackResist,
      ExtraValue,
      ExtraValuePosition,
      _definition.MaximumLife != _definition.ClientMaximumLife);
  }

  private void ApplyState(NpcStateUpdate update)
  {
    Position = update.Position;
    Velocity = update.Velocity;
    Target = update.Target;
    DirectionPositive = update.DirectionPositive;
    DirectionYPositive = update.DirectionYPositive;
    SpriteDirectionPositive = update.SpriteDirectionPositive;
    Ai = Array.AsReadOnly(update.Ai.ToArray());
    if (Ai[1] != 0f) {
      ClearSlimeBodyItem();
    }
    Life = update.Life;
    _maximumLife = update.MaximumLife;
    CatchableReleaseOwner = update.CatchableReleaseOwner;
    SpawnedFromStatue = update.SpawnedFromStatue;
    SpawnNeedsSyncing = update.SpawnNeedsSyncing;
    Shimmering = update.Shimmering;
    PlayerCount = update.PlayerCount;
    Difficulty = update.Difficulty;
  }
}
