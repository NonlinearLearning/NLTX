using Terraria.Relationships;

namespace Terraria.Items;

public sealed class ItemUseComponent
{
  public ItemUseComponent(
    ItemUsePhase phase = ItemUsePhase.Idle,
    long startedAtTick = 0,
    long cooldownUntilTick = 0,
    EntityReference owner = default,
    EntityReference targetEntity = default,
    TileCoordinates? targetTile = null,
    long useSequence = 0)
  {
    Phase = phase;
    StartedAtTick = startedAtTick;
    CooldownUntilTick = cooldownUntilTick;
    Owner = owner;
    TargetEntity = targetEntity;
    TargetTile = targetTile;
    UseSequence = useSequence;
  }

  public ItemUsePhase Phase;
  public long StartedAtTick;
  public long CooldownUntilTick;
  public EntityReference Owner;
  public EntityReference TargetEntity;
  public TileCoordinates? TargetTile;
  public long UseSequence;

  public bool IsActive => Phase is ItemUsePhase.Starting or ItemUsePhase.Using or ItemUsePhase.Channeling;
  public bool IsCoolingDown => Phase == ItemUsePhase.Cooldown && CooldownUntilTick > StartedAtTick;
}
