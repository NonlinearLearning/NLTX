using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Combat.Commands;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Player.Commands;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.StatusEffects.Commands;
using Terraria.Dome.Simulation.Wiring.Commands;

namespace Terraria.Dome.Simulation.Tick;

public sealed class SimulationCommandQueue
{
  private readonly List<DamageCommand> _damageCommands = new();
  private readonly List<DamageNpcCommand> _damageNpcCommands = new();
  private readonly List<DespawnEntityCommand> _despawnEntityCommands = new();
  private readonly List<ProjectileWorldObjectPlacementCommand>
    _projectileWorldObjectPlacementCommands = new();
  private readonly List<BounceProjectileCommand> _bounceProjectileCommands = new();
  private readonly List<SpawnProjectileCommand> _spawnProjectileCommands = new();
  private readonly List<DamagePlayerCommand> _damagePlayerCommands = new();
  private readonly List<RespawnPlayerCommand> _respawnPlayerCommands = new();
  private readonly List<ApplyShadowDodgeCommand> _shadowDodgeCommands = new();
  private readonly List<PickupWorldItemCommand> _pickupWorldItemCommands = new();
  private readonly List<MoveWorldItemCommand> _moveWorldItemCommands = new();
  private readonly List<DestroyWorldItemCommand> _destroyWorldItemCommands = new();
  private readonly List<TransferItemCommand> _transferItemCommands = new();
  private readonly List<SplitItemStackCommand> _splitItemStackCommands = new();
  private readonly List<MergeItemStackCommand> _mergeItemStackCommands = new();
  private readonly List<DropItemCommand> _dropItemCommands = new();
  private readonly List<UseItemCommand> _useItemCommands = new();
  private readonly List<UseExtractinatorCommand> _useExtractinatorCommands = new();
  private readonly List<TriggerExtractinatorCommand> _triggerExtractinatorCommands = new();
  private readonly List<UsePlayerInteractionCommand> _usePlayerInteractionCommands = new();
  private readonly List<NpcInteractionCommand> _npcInteractionCommands = new();
  private readonly List<ShopPurchaseCommand> _shopPurchaseCommands = new();
  private readonly List<LiquidChangeCommand> _liquidChangeCommands = new();
  private readonly List<LiquidTransferCommand> _liquidTransferCommands = new();
  private readonly List<MechanismActivationCommand> _mechanismActivationCommands = new();
  private readonly List<SpawnNpcCommand> _spawnNpcCommands = new();
  private readonly List<DespawnNpcCommand> _despawnNpcCommands = new();
  private readonly List<ApplyAreaStatusEffectCommand> _areaStatusEffectCommands = new();
  private readonly List<ApplyTargetStatusEffectCommand> _targetStatusEffectCommands = new();

  public IReadOnlyList<DamageCommand> DamageCommands => _damageCommands;
  public IReadOnlyList<DamageNpcCommand> DamageNpcCommands => _damageNpcCommands;
  public IReadOnlyList<DespawnEntityCommand> DespawnEntityCommands => _despawnEntityCommands;
  public IReadOnlyList<ProjectileWorldObjectPlacementCommand>
    ProjectileWorldObjectPlacementCommands =>
    _projectileWorldObjectPlacementCommands;
  public IReadOnlyList<BounceProjectileCommand> BounceProjectileCommands =>
    _bounceProjectileCommands;
  public IReadOnlyList<SpawnProjectileCommand> SpawnProjectileCommands => _spawnProjectileCommands;
  public IReadOnlyList<DamagePlayerCommand> DamagePlayerCommands => _damagePlayerCommands;
  public IReadOnlyList<RespawnPlayerCommand> RespawnPlayerCommands => _respawnPlayerCommands;
  public IReadOnlyList<ApplyShadowDodgeCommand> ShadowDodgeCommands => _shadowDodgeCommands;
  public IReadOnlyList<PickupWorldItemCommand> PickupWorldItemCommands => _pickupWorldItemCommands;
  public IReadOnlyList<MoveWorldItemCommand> MoveWorldItemCommands => _moveWorldItemCommands;
  public IReadOnlyList<DestroyWorldItemCommand> DestroyWorldItemCommands =>
    _destroyWorldItemCommands;
  public IReadOnlyList<TransferItemCommand> TransferItemCommands => _transferItemCommands;
  public IReadOnlyList<SplitItemStackCommand> SplitItemStackCommands => _splitItemStackCommands;
  public IReadOnlyList<MergeItemStackCommand> MergeItemStackCommands => _mergeItemStackCommands;
  public IReadOnlyList<DropItemCommand> DropItemCommands => _dropItemCommands;
  public IReadOnlyList<UseItemCommand> UseItemCommands => _useItemCommands;
  public IReadOnlyList<UseExtractinatorCommand> UseExtractinatorCommands =>
    _useExtractinatorCommands;
  public IReadOnlyList<TriggerExtractinatorCommand> TriggerExtractinatorCommands =>
    _triggerExtractinatorCommands;
  public IReadOnlyList<UsePlayerInteractionCommand> UsePlayerInteractionCommands =>
    _usePlayerInteractionCommands;
  public IReadOnlyList<NpcInteractionCommand> NpcInteractionCommands => _npcInteractionCommands;
  public IReadOnlyList<ShopPurchaseCommand> ShopPurchaseCommands
  {
    get
    {
      _shopPurchaseCommands.Sort(ShopPurchaseCommandComparer.Instance);
      return _shopPurchaseCommands;
    }
  }
  public IReadOnlyList<LiquidChangeCommand> LiquidChangeCommands
  {
    get
    {
      _liquidChangeCommands.Sort(LiquidChangeCommandComparer.Instance);
      return _liquidChangeCommands;
    }
  }

  public IReadOnlyList<LiquidTransferCommand> LiquidTransferCommands
  {
    get
    {
      _liquidTransferCommands.Sort(LiquidTransferCommandComparer.Instance);
      return _liquidTransferCommands;
    }
  }

  public IReadOnlyList<MechanismActivationCommand> MechanismActivationCommands
  {
    get
    {
      _mechanismActivationCommands.Sort(MechanismActivationCommandComparer.Instance);
      return _mechanismActivationCommands;
    }
  }

  public IReadOnlyList<SpawnNpcCommand> SpawnNpcCommands => _spawnNpcCommands;
  public IReadOnlyList<DespawnNpcCommand> DespawnNpcCommands => _despawnNpcCommands;
  public IReadOnlyList<ApplyAreaStatusEffectCommand> AreaStatusEffectCommands =>
    _areaStatusEffectCommands;
  public IReadOnlyList<ApplyTargetStatusEffectCommand> TargetStatusEffectCommands =>
    _targetStatusEffectCommands;

  public int Count =>
    _damageCommands.Count +
    _damageNpcCommands.Count +
    _despawnEntityCommands.Count +
    _projectileWorldObjectPlacementCommands.Count +
    _bounceProjectileCommands.Count +
    _spawnProjectileCommands.Count +
    _damagePlayerCommands.Count +
    _respawnPlayerCommands.Count +
    _shadowDodgeCommands.Count +
    _pickupWorldItemCommands.Count +
    _moveWorldItemCommands.Count +
    _destroyWorldItemCommands.Count +
    _transferItemCommands.Count +
    _splitItemStackCommands.Count +
    _mergeItemStackCommands.Count +
    _dropItemCommands.Count +
    _useItemCommands.Count +
    _useExtractinatorCommands.Count +
    _triggerExtractinatorCommands.Count +
    _usePlayerInteractionCommands.Count +
    _npcInteractionCommands.Count +
    _shopPurchaseCommands.Count +
    _liquidChangeCommands.Count +
    _liquidTransferCommands.Count +
    _mechanismActivationCommands.Count +
    _spawnNpcCommands.Count +
    _despawnNpcCommands.Count +
    _areaStatusEffectCommands.Count +
    _targetStatusEffectCommands.Count;

  public void Clear()
  {
    _damageCommands.Clear();
    _damageNpcCommands.Clear();
    _despawnEntityCommands.Clear();
    _projectileWorldObjectPlacementCommands.Clear();
    _bounceProjectileCommands.Clear();
    _spawnProjectileCommands.Clear();
    _damagePlayerCommands.Clear();
    _respawnPlayerCommands.Clear();
    _shadowDodgeCommands.Clear();
    _pickupWorldItemCommands.Clear();
    _moveWorldItemCommands.Clear();
    _destroyWorldItemCommands.Clear();
    _transferItemCommands.Clear();
    _splitItemStackCommands.Clear();
    _mergeItemStackCommands.Clear();
    _dropItemCommands.Clear();
    _useItemCommands.Clear();
    _useExtractinatorCommands.Clear();
    _triggerExtractinatorCommands.Clear();
    _usePlayerInteractionCommands.Clear();
    _npcInteractionCommands.Clear();
    _shopPurchaseCommands.Clear();
    _liquidChangeCommands.Clear();
    _liquidTransferCommands.Clear();
    _mechanismActivationCommands.Clear();
    _spawnNpcCommands.Clear();
    _despawnNpcCommands.Clear();
    _areaStatusEffectCommands.Clear();
    _targetStatusEffectCommands.Clear();
  }

  public void Enqueue(DamageCommand command)
  {
    _damageCommands.Add(command);
  }

  public void Enqueue(DamageNpcCommand command)
  {
    _damageNpcCommands.Add(command);
  }

  public void Enqueue(DespawnEntityCommand command)
  {
    _despawnEntityCommands.Add(command);
  }

  public void Enqueue(ProjectileWorldObjectPlacementCommand command)
  {
    ValidateSequence(command.Sequence);
    _projectileWorldObjectPlacementCommands.Add(command);
  }

  public void Enqueue(BounceProjectileCommand command)
  {
    _bounceProjectileCommands.Add(command);
  }

  public void Enqueue(SpawnProjectileCommand command)
  {
    _spawnProjectileCommands.Add(command);
  }

  public void Enqueue(DamagePlayerCommand command)
  {
    _damagePlayerCommands.Add(command);
  }

  public void Enqueue(RespawnPlayerCommand command)
  {
    _respawnPlayerCommands.Add(command);
  }

  public void Enqueue(ApplyShadowDodgeCommand command)
  {
    _shadowDodgeCommands.Add(command);
  }

  public void Enqueue(PickupWorldItemCommand command)
  {
    _pickupWorldItemCommands.Add(command);
  }

  public void Enqueue(MoveWorldItemCommand command)
  {
    _moveWorldItemCommands.Add(command);
  }

  public void Enqueue(DestroyWorldItemCommand command)
  {
    _destroyWorldItemCommands.Add(command);
  }

  public void Enqueue(TransferItemCommand command)
  {
    _transferItemCommands.Add(command);
  }

  public void Enqueue(SplitItemStackCommand command)
  {
    _splitItemStackCommands.Add(command);
  }

  public void Enqueue(MergeItemStackCommand command)
  {
    _mergeItemStackCommands.Add(command);
  }

  public void Enqueue(DropItemCommand command)
  {
    _dropItemCommands.Add(command);
  }

  public void Enqueue(UseItemCommand command)
  {
    ValidateSequence(command.Sequence);
    _useItemCommands.Add(command);
  }

  public void Enqueue(UseExtractinatorCommand command)
  {
    ValidateSequence(command.Sequence);
    _useExtractinatorCommands.Add(command);
  }

  public void Enqueue(TriggerExtractinatorCommand command)
  {
    ValidateSequence(command.Sequence);
    _triggerExtractinatorCommands.Add(command);
  }

  public void Enqueue(UsePlayerInteractionCommand command)
  {
    _usePlayerInteractionCommands.Add(command);
  }

  public void Enqueue(NpcInteractionCommand command)
  {
    _npcInteractionCommands.Add(command);
  }

  public void Enqueue(ShopPurchaseCommand command)
  {
    ValidateSequence(command.Sequence);
    _shopPurchaseCommands.Add(command);
  }

  public void Enqueue(LiquidChangeCommand command)
  {
    ValidateSequence(command.Sequence);
    _liquidChangeCommands.Add(command);
  }

  public void Enqueue(LiquidTransferCommand command)
  {
    ValidateSequence(command.Sequence);
    _liquidTransferCommands.Add(command);
  }

  public void Enqueue(MechanismActivationCommand command)
  {
    ValidateSequence(command.Sequence);
    _mechanismActivationCommands.Add(command);
  }

  public void Enqueue(SpawnNpcCommand command)
  {
    _spawnNpcCommands.Add(command);
  }

  public void Enqueue(DespawnNpcCommand command)
  {
    _despawnNpcCommands.Add(command);
  }

  public void Enqueue(ApplyAreaStatusEffectCommand command)
  {
    command.Validate();
    _areaStatusEffectCommands.Add(command);
  }

  public void Enqueue(ApplyTargetStatusEffectCommand command)
  {
    command.Validate();
    _targetStatusEffectCommands.Add(command);
  }

  private static void ValidateSequence(long sequence)
  {
    if (sequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sequence));
    }
  }

  private sealed class LiquidChangeCommandComparer : IComparer<LiquidChangeCommand>
  {
    public static readonly LiquidChangeCommandComparer Instance = new();

    public int Compare(LiquidChangeCommand first, LiquidChangeCommand second)
    {
      int sequence = first.Sequence.CompareTo(second.Sequence);
      if (sequence != 0)
      {
        return sequence;
      }

      int x = first.X.CompareTo(second.X);
      if (x != 0)
      {
        return x;
      }

      int y = first.Y.CompareTo(second.Y);
      if (y != 0)
      {
        return y;
      }

      int type = first.Type.CompareTo(second.Type);
      return type != 0 ? type : first.Amount.CompareTo(second.Amount);
    }
  }

  private sealed class LiquidTransferCommandComparer : IComparer<LiquidTransferCommand>
  {
    public static readonly LiquidTransferCommandComparer Instance = new();

    public int Compare(LiquidTransferCommand first, LiquidTransferCommand second)
    {
      int sequence = first.Sequence.CompareTo(second.Sequence);
      if (sequence != 0)
      {
        return sequence;
      }

      int sourceX = first.SourceX.CompareTo(second.SourceX);
      if (sourceX != 0)
      {
        return sourceX;
      }

      int sourceY = first.SourceY.CompareTo(second.SourceY);
      if (sourceY != 0)
      {
        return sourceY;
      }

      int targetX = first.TargetX.CompareTo(second.TargetX);
      return targetX != 0 ? targetX : first.TargetY.CompareTo(second.TargetY);
    }
  }

  private sealed class MechanismActivationCommandComparer :
    IComparer<MechanismActivationCommand>
  {
    public static readonly MechanismActivationCommandComparer Instance = new();

    public int Compare(MechanismActivationCommand first, MechanismActivationCommand second)
    {
      return SimulationTickSchedule.CompareMechanismActivationCommands(first, second);
    }
  }

  private sealed class ShopPurchaseCommandComparer : IComparer<ShopPurchaseCommand>
  {
    public static readonly ShopPurchaseCommandComparer Instance = new();

    public int Compare(ShopPurchaseCommand first, ShopPurchaseCommand second)
    {
      int sequence = first.Sequence.CompareTo(second.Sequence);
      if (sequence != 0)
      {
        return sequence;
      }

      int player = first.Player.Value.CompareTo(second.Player.Value);
      if (player != 0)
      {
        return player;
      }

      int npc = first.Npc.Value.CompareTo(second.Npc.Value);
      return npc != 0 ? npc : first.OfferId.CompareTo(second.OfferId);
    }
  }
}
