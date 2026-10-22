using System;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Captures Version4 facts from PlayerOwner, InventoryStore and versioned item/world sources.
/// Equipment effects and reach are calculated here; adapters provide definitions, not results.
/// </summary>
public sealed class PlayerRuntimeFactsCaptureService
{
  private readonly PlayerOwner _players;
  private readonly InventoryStore _inventories;
  private readonly IPlayerEquipmentMetadataSource _equipmentMetadata;
  private readonly IPlayerRuntimeWorldCaptureSource _worldCapture;

  public PlayerRuntimeFactsCaptureService(
    PlayerOwner players,
    InventoryStore inventories,
    IPlayerEquipmentMetadataSource equipmentMetadata,
    IPlayerRuntimeWorldCaptureSource worldCapture)
  {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(equipmentMetadata);
    ArgumentNullException.ThrowIfNull(worldCapture);
    _players = players;
    _inventories = inventories;
    _equipmentMetadata = equipmentMetadata;
    _worldCapture = worldCapture;
  }

  public OwnerCommitResult TryCaptureSpatial(
    SessionBinding binding,
    Player player,
    out IPlayerSpatialFactsCapture? capture)
  {
    capture = null;
    OwnerCommitResult result = TryCaptureInputs(binding, player, out CapturedPlayerFacts? inputs);
    if (result.Status != OwnerCommitStatus.Applied || inputs is null)
    {
      return result;
    }
    if (inputs.Calculation.HasPortableStool && !player.RuntimeSpatial.PortableStoolStateKnown)
    {
      return Unavailable("Portable-stool clearance and runtime use state have not been captured.");
    }
    if (player.RuntimeSpatial.PortableStoolHas != inputs.Calculation.HasPortableStool)
    {
      return Unavailable("The equipped portable-stool state has not been committed to Player.");
    }

    var prepared = new SpatialFactsCapture(inputs);
    result = prepared.Validate();
    if (result.Status == OwnerCommitStatus.Applied)
    {
      capture = prepared;
    }
    return result;
  }

  public OwnerCommitResult TryCaptureTickFacts(
    SessionBinding binding,
    Player player,
    out IPlayerTickWorldFacts? capture)
  {
    capture = null;
    if (binding.Epoch != player.Epoch || binding.PlayerHandle != player.Handle
      || !_players.TryGet(binding.PlayerHandle, out Player? activePlayer)
      || !ReferenceEquals(activePlayer, player))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The Player tick binding is no longer active.");
    }
    IPlayerRuntimeWorldCapture? world = _worldCapture.Capture(binding, player);
    if (world is null)
    {
      return Unavailable("The same-version world dayRate and LanternsUp facts are unavailable.");
    }
    if (world.Epoch != binding.Epoch || world.PlayerHandle != player.Handle
      || world.PlayerVersion != player.Version || !world.Revision.IsValid || world.DayRate < 0)
    {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The world tick facts do not match the active Player version.");
    }
    OwnerCommitResult validation = world.Validate();
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return validation;
    }
    capture = new RuntimeTickWorldFacts(world);
    return OwnerCommitResult.Applied(player.Version);
  }

  public OwnerCommitResult TryPrepareMovement(
    SessionBinding binding,
    Player player,
    PlayerMovementInput movement,
    PlayerControlsPacket packet,
    out PlayerRuntimeMovementPreparation? preparation)
  {
    ArgumentNullException.ThrowIfNull(binding);
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(movement);
    ArgumentNullException.ThrowIfNull(packet);
    preparation = null;
    if (!_players.TryGet(binding.PlayerHandle, out Player? current)
      || !ReferenceEquals(current, player) || binding.Epoch != player.Epoch)
    {
      return new OwnerCommitResult(OwnerCommitStatus.Rejected, null, ProtocolErrorCode.StaleEntity,
        "The Player controls binding is no longer active.");
    }
    if (!_inventories.TryCapture(player.Handle.Id, out InventorySnapshot? inventory)
      || inventory is null || !IsInventorySnapshotValid(
        inventory, player.Handle.Id, InventorySlotLayout.ArmorStart + 10))
    {
      return Unavailable("The Player equipment slots are unavailable.");
    }
    if (!PlayerRuntimeSpatialStateCatalog.TryCreateCollisionCandidate(
      player.RuntimeSpatial, packet, movement, out PlayerRuntimeSpatialState collisionCandidate,
      out PlayerMovementInput collisionMovement))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The explicit mount command is outside the Version4 mount range.");
    }
    IPlayerRuntimeWorldCapture? world = _worldCapture.Capture(
      binding, player, collisionMovement, collisionCandidate);
    if (world is null || world.Epoch != binding.Epoch || world.PlayerHandle != player.Handle
      || world.PlayerVersion != player.Version || !world.Revision.IsValid)
    {
      return Unavailable("The mount, stool and collision inputs are unavailable.");
    }
    OwnerCommitResult metadataResult = TryCapturePortableStoolEquipped(
      binding, player, inventory, world, out bool hasStool, out CommitVersion metadataRevision);
    if (metadataResult.Status != OwnerCommitStatus.Applied)
    {
      return metadataResult;
    }
    if (!PlayerRuntimeSpatialStateCatalog.TryApplyMovementCommand(
      player.RuntimeSpatial, packet, collisionMovement, world, out PlayerRuntimeSpatialState runtime,
      out PlayerMovementInput updatedMovement, out bool unavailable))
    {
      return unavailable
        ? Unavailable("The explicit mount or dismount collision facts are unavailable.")
        : OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
          "The explicit mount command is outside the Version4 mount range.");
    }

    bool up = (movement.ControlFlags & 0b0000_0001) != 0;
    bool stoolCandidate = hasStool && up && !runtime.MountActive
      && movement.Velocity == default && !runtime.Pulley;
    if (stoolCandidate && (runtime.GravityControlled is null || !runtime.GrapplingStateKnown))
    {
      return Unavailable("Player gravity-control or grapple state is not authoritative yet.");
    }
    bool stoolEligible = stoolCandidate && runtime.GravityControlled == false
      && runtime.FirstGrapplingProjectile is null;
    if (stoolEligible)
    {
      IPlayerRuntimeWorldCapture? stoolWorld = _worldCapture.Capture(
        binding, player, updatedMovement, runtime);
      if (stoolWorld is null || stoolWorld.Epoch != binding.Epoch
        || stoolWorld.PlayerHandle != player.Handle || stoolWorld.PlayerVersion != player.Version
        || stoolWorld.Revision != world.Revision || stoolWorld.CanFitPortableStool is null)
      {
        return Unavailable("Portable-stool clearance is unavailable for the prepared collider.");
      }
      OwnerCommitResult stoolValidation = stoolWorld.Validate();
      if (stoolValidation.Status != OwnerCommitStatus.Applied)
      {
        return stoolValidation;
      }
      world = stoolWorld;
    }
    bool stoolInUse = stoolEligible && world.CanFitPortableStool == true;
    int width = runtime.MountActive ? runtime.Width : 20;
    int height = runtime.MountActive ? runtime.Height : stoolInUse ? 68 : 42;
    if (runtime.Height != height)
    {
      updatedMovement = updatedMovement with
      {
        Position = new Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketVector2(
          updatedMovement.Position.X, updatedMovement.Position.Y + runtime.Height - height)
      };
    }
    runtime = runtime with
    {
      Width = width,
      Height = height,
      PortableStoolHas = hasStool,
      PortableStoolInUse = stoolInUse,
      PortableStoolStateKnown = true
    };

    OwnerCommitResult validation = world.Validate();
    if (validation.Status != OwnerCommitStatus.Applied)
    {
      return validation;
    }
    preparation = new PlayerRuntimeMovementPreparation(
      _players, binding, player, updatedMovement, runtime, world, _equipmentMetadata,
      metadataRevision, _inventories, inventory.Revision);
    return preparation.Validate();
  }

  public OwnerCommitResult TryCapturePickup(
    SessionBinding binding,
    Player player,
    out IPlayerPickupFactsCapture? capture)
  {
    capture = null;
    OwnerCommitResult result = TryCaptureInputs(binding, player, out CapturedPlayerFacts? inputs);
    if (result.Status != OwnerCommitStatus.Applied || inputs is null)
    {
      return result;
    }
    if (!inputs.Calculation.SelectedItemValid)
    {
      return Unavailable("The selected Player inventory slot is unavailable.");
    }
    if (!player.RuntimeSpatial.ItemAnimationKnown)
    {
      return Unavailable("The authoritative item-animation state is not initialized.");
    }
    if (inputs.Calculation.HasPortableStool && !player.RuntimeSpatial.PortableStoolStateKnown)
    {
      return Unavailable("Portable-stool clearance and runtime use state have not been captured.");
    }
    if (player.RuntimeSpatial.PortableStoolHas != inputs.Calculation.HasPortableStool)
    {
      return Unavailable("The equipped portable-stool state has not been committed to Player.");
    }
    var prepared = new PickupFactsCapture(inputs);
    result = prepared.Validate();
    if (result.Status == OwnerCommitStatus.Applied)
    {
      capture = prepared;
    }
    return result;
  }

  /// <summary>
  /// Captures ownership acceptance, magnets and grab range without requiring item-use or
  /// portable-stool geometry readiness. FindOwner captures actual geometry separately.
  /// </summary>
  public OwnerCommitResult TryCaptureOwnership(
    SessionBinding binding,
    Player player,
    out IPlayerPickupFactsCapture? capture) {
    capture = null;
    OwnerCommitResult result = TryCaptureInputs(binding, player, out CapturedPlayerFacts? inputs);
    if (result.Status != OwnerCommitStatus.Applied || inputs is null) {
      return result;
    }

    var prepared = new PickupFactsCapture(inputs);
    result = prepared.Validate();
    if (result.Status == OwnerCommitStatus.Applied) {
      capture = prepared;
    }
    return result;
  }

  internal OwnerCommitResult TryCapturePortableStoolEquipped(
    SessionBinding binding,
    Player player,
    InventorySnapshot inventory,
    IPlayerRuntimeWorldCapture world,
    out bool hasStool,
    out CommitVersion metadataRevision)
  {
    hasStool = false;
    metadataRevision = default;
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(world);
    if (binding.Epoch != player.Epoch || binding.PlayerHandle != player.Handle
      || world.Epoch != binding.Epoch || world.PlayerHandle != player.Handle
      || world.PlayerVersion != player.Version
      || !IsInventorySnapshotValid(
        inventory, player.Handle.Id, InventorySlotLayout.ArmorStart + 10))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
        "The portable-stool sources belong to another Player version.");
    }
    if (!_equipmentMetadata.Revision.IsValid)
    {
      return Unavailable("The Version4 item metadata revision is unavailable.");
    }
    metadataRevision = _equipmentMetadata.Revision;
    for (int slot = 3; slot < 10; slot++)
    {
      if (!IsEquipmentSlotUsable(slot, player, world))
      {
        continue;
      }
      InventoryItemState item = ItemAt(inventory, InventorySlotLayout.ArmorStart + slot);
      if (item.IsEmpty || item.Type is not (4341 or 5126))
      {
        continue;
      }
      if (!_equipmentMetadata.TryGet(item.Type, out PlayerEquipmentItemFacts? metadata)
        || metadata is null)
      {
        return Unavailable("Portable-stool item metadata is unavailable.");
      }
      if (!metadata.ExpertOnly || world.ExpertMode)
      {
        hasStool |= metadata.IsAccessory;
      }
    }
    if (_equipmentMetadata.Revision != metadataRevision)
    {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The equipment metadata changed during portable-stool capture.");
    }
    return OwnerCommitResult.Applied(player.Version);
  }

  private OwnerCommitResult TryCaptureInputs(
    SessionBinding binding,
    Player player,
    out CapturedPlayerFacts? inputs)
  {
    inputs = null;
    if (binding.Epoch != player.Epoch || binding.PlayerHandle != player.Handle
      || !_players.TryGet(binding.PlayerHandle, out Player? currentPlayer)
      || !ReferenceEquals(currentPlayer, player))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The Player facts binding is no longer active.");
    }
    if (!_inventories.TryCapture(player.Handle.Id, out InventorySnapshot? inventory)
      || inventory is null || !IsInventorySnapshotValid(
        inventory, player.Handle.Id, InventorySlotLayout.ArmorStart + 10))
    {
      return Unavailable("The authoritative inventory and equipped slots are unavailable.");
    }
    if (!inventory.Revision.IsValid || !_equipmentMetadata.Revision.IsValid)
    {
      return Unavailable("The inventory or item-metadata revision is unavailable.");
    }

    IPlayerRuntimeWorldCapture? world = _worldCapture.Capture(binding, player);
    if (world is null)
    {
      return Unavailable("The Player's current world and creative-power facts are unavailable.");
    }
    if (world.Epoch != binding.Epoch || world.PlayerHandle != player.Handle
      || world.PlayerVersion != player.Version || !world.Revision.IsValid)
    {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The world facts do not match the current Player version.");
    }

    CommitVersion equipmentRevision = _equipmentMetadata.Revision;
    if (!TryComputeEquipment(player, inventory, world, out PlayerEquipmentCalculation? calculation))
    {
      return Unavailable("The equipped-item metadata needed for Version4 effects is incomplete.");
    }
    if (_equipmentMetadata.Revision != equipmentRevision)
    {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The equipment metadata changed during Player facts capture.");
    }

    OwnerCommitResult worldValidation = world.Validate();
    if (worldValidation.Status != OwnerCommitStatus.Applied)
    {
      return worldValidation;
    }
    var revision = new PlayerSourceRevision(inventory.Revision, equipmentRevision, world.Revision);
    inputs = new CapturedPlayerFacts(
      _players, _inventories, _equipmentMetadata, inventory, world, player, calculation!, revision);
    return OwnerCommitResult.Applied(player.Version);
  }

  private bool TryComputeEquipment(
    Player player,
    InventorySnapshot inventory,
    IPlayerRuntimeWorldCapture world,
    out PlayerEquipmentCalculation? calculation)
  {
    calculation = null;
    int effectiveMana = player.Vitals.MaximumMana;
    int tileRangeX = 5;
    int tileRangeY = 3;
    if (world.JourneyWorld && world.FarPlacementPowerUnlocked && world.FarPlacementPowerEnabled)
    {
      tileRangeX = tileRangeX * 2 + 8;
      tileRangeY = tileRangeY * 2 + 8;
    }

    bool hasPortableStool = false;
    bool goldRing = false;
    bool manaMagnet = false;
    bool treasureMagnet = false;
    bool preventAllPickups = false;
    int headSlot = -1;
    int bodySlot = -1;

    for (int slot = 0; slot < 10; slot++)
    {
      InventoryItemState item = ItemAt(inventory, InventorySlotLayout.ArmorStart + slot);
      if (item.IsEmpty || !IsEquipmentSlotUsable(slot, player, world))
      {
        continue;
      }
      if (!_equipmentMetadata.TryGet(item.Type, out PlayerEquipmentItemFacts? metadata)
        || metadata is null)
      {
        return false;
      }
      if (metadata.ExpertOnly && !world.ExpertMode)
      {
        continue;
      }

      bool grantsArmor = slot switch
      {
        0 => metadata.HeadSlot >= 0,
        1 => metadata.BodySlot >= 0,
        2 => metadata.LegSlot >= 0,
        _ => metadata.IsAccessory
      };
      if (!grantsArmor)
      {
        continue;
      }

      if (slot == 0)
      {
        headSlot = metadata.HeadSlot;
      }
      else if (slot == 1)
      {
        bodySlot = metadata.BodySlot;
      }

      effectiveMana += GetArmorManaMaximumBonus(item.Type);
      if (metadata.IsAccessory)
      {
        if (item.Prefix == 66)
        {
          effectiveMana += 20;
        }
        effectiveMana += GetAccessoryManaMaximumBonus(item.Type);
      }

      if (slot < 3)
      {
        continue;
      }

      preventAllPickups |= metadata.IsAccessory && item.Type == 4346;
      goldRing |= item.Type is 3033 or 3034 or 3035;
      manaMagnet |= item.Type is 2219 or 2220 or 2221 or 4000;
      treasureMagnet |= item.Type is 5010 or 5126;
      hasPortableStool |= item.Type is 4341 or 5126;
      if (item.Type == 1923)
      {
        tileRangeX++;
        tileRangeY++;
      }
      if (item.Type is 2215 or 3061 or 5126)
      {
        tileRangeX += 3;
        tileRangeY += 2;
      }
    }

    if (headSlot == 159 && (bodySlot is >= 58 and <= 63 or 167 or 213))
    {
      effectiveMana += 60;
    }
    if (player.Buffs.HasBuff(29))
    {
      effectiveMana += 20;
    }
    effectiveMana = Math.Min(400, effectiveMana);

    bool ownsVoidVault = false;
    for (int slot = 0; slot < 58; slot++)
    {
      int itemType = ItemAt(inventory, slot).Type;
      ownsVoidVault |= itemType == 4131;
    }

    bool selectedItemValid = player.Movement.SelectedItem < InventorySlotLayout.Mouse;
    bool itemUseStateKnown = selectedItemValid && player.RuntimeSpatial.ItemAnimationKnown;
    InventoryItemState heldItem = selectedItemValid
      ? ItemAt(inventory, player.Movement.SelectedItem)
      : InventoryItemState.Empty;
    bool journeyFarPlacementEnabled = world.JourneyWorld && world.FarPlacementPowerEnabled;
    int effectiveLife = player.Vitals.GetEffectiveMaximumLife(player.Buffs);

    PlayerRuntimeSpatialState runtime = player.RuntimeSpatial;
    if (runtime.Width <= 0 || runtime.Height <= 0)
    {
      return false;
    }
    var spatial = new PlayerSpatialSourceFacts(
      runtime.Width,
      runtime.Height,
      runtime.MountActive,
      runtime.MountHeightBoost,
      runtime.MountUsesSizeOverride,
      runtime.PortableStoolInUse,
      runtime.PortableStoolInUse ? 26 : 0,
      tileRangeX,
      tileRangeY);
    var pickup = new PlayerPickupSourceFacts(
      preventAllPickups,
      ownsVoidVault && runtime.VoidVaultEnabled,
      selectedItemValid && heldItem.IsEmpty,
      itemUseStateKnown ? runtime.ItemAnimation : -1,
      effectiveLife,
      effectiveMana,
      goldRing,
      manaMagnet,
      player.Buffs.HasBuff(105),
      treasureMagnet,
      journeyFarPlacementEnabled);
    calculation = new PlayerEquipmentCalculation(
      spatial, pickup, hasPortableStool, selectedItemValid);
    return true;
  }

  private static bool IsEquipmentSlotUsable(
    int slot,
    Player player,
    IPlayerRuntimeWorldCapture world)
  {
    return slot switch
    {
      8 => player.Appearance.HasExtraAccessorySlot && world.ExpertMode,
      9 => world.MasterMode,
      _ => true
    };
  }

  private static InventoryItemState ItemAt(InventorySnapshot snapshot, int slot)
  {
    return (uint)slot < (uint)snapshot.Slots.Count
      ? snapshot.Slots[slot].Item
      : InventoryItemState.Empty;
  }

  internal static bool IsInventorySnapshotValid(
    InventorySnapshot? snapshot,
    EntityUuid expectedPlayerId,
    int requiredSlotCount)
  {
    if (snapshot is null || snapshot.PlayerId != expectedPlayerId || !snapshot.Revision.IsValid
      || snapshot.Slots.Count < requiredSlotCount)
    {
      return false;
    }

    for (int index = 0; index < snapshot.Slots.Count; index++)
    {
      InventorySlotSnapshot slot = snapshot.Slots[index];
      if (slot is null || slot.SlotId != index || !slot.Generation.IsValid)
      {
        return false;
      }
    }
    return true;
  }

  private static int GetAccessoryManaMaximumBonus(int itemType)
  {
    return itemType is 111 or 982 or 1595 or 2221 ? 20 : 0;
  }

  private static int GetArmorManaMaximumBonus(int itemType)
  {
    return itemType switch
    {
      371 or 3777 or 3778 or 228 or 960 => 40,
      229 or 961 or 230 or 962 or 1282 => 20,
      376 or 1207 or 2189 or 1285 or 2760 => 60,
      400 or 1212 or 1003 or 1287 => 80,
      1217 or 558 or 4898 => 100,
      1283 or 1284 => 40,
      1286 or 4256 => 60,
      _ => 0
    };
  }

  private static OwnerCommitResult Unavailable(string detail)
  {
    return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
      detail);
  }

  private sealed record PlayerEquipmentCalculation(
    PlayerSpatialSourceFacts Spatial,
    PlayerPickupSourceFacts Pickup,
    bool HasPortableStool,
    bool SelectedItemValid);

  private sealed class CapturedPlayerFacts
  {
    private readonly PlayerOwner _players;
    private readonly InventoryStore _inventories;
    private readonly IPlayerEquipmentMetadataSource _equipmentMetadata;
    private readonly InventorySnapshot _inventory;
    private readonly IPlayerRuntimeWorldCapture _world;
    private readonly Player _player;
    private readonly PlayerRuntimeSpatialState _runtimeState;

    public CapturedPlayerFacts(
      PlayerOwner players,
      InventoryStore inventories,
      IPlayerEquipmentMetadataSource equipmentMetadata,
      InventorySnapshot inventory,
      IPlayerRuntimeWorldCapture world,
      Player player,
      PlayerEquipmentCalculation calculation,
      PlayerSourceRevision revision)
    {
      _players = players;
      _inventories = inventories;
      _equipmentMetadata = equipmentMetadata;
      _inventory = inventory;
      _world = world;
      _player = player;
      _runtimeState = player.RuntimeSpatial;
      Calculation = calculation;
      Epoch = player.Epoch;
      PlayerHandle = player.Handle;
      PlayerVersion = player.Version;
      SourceRevision = revision;
    }

    public SessionEpoch Epoch { get; }

    public EntityHandle PlayerHandle { get; }

    public CommitVersion PlayerVersion { get; }

    public PlayerSourceRevision SourceRevision { get; }

    public PlayerEquipmentCalculation Calculation { get; }

    public IPlayerRuntimeWorldCapture World => _world;

    public OwnerCommitResult Validate()
    {
      if (!_players.TryGet(PlayerHandle, out Player? currentPlayer)
        || !ReferenceEquals(currentPlayer, _player) || _player.Epoch != Epoch
        || _player.Handle != PlayerHandle || _player.Version != PlayerVersion
        || _player.RuntimeSpatial != _runtimeState
        || !_inventories.TryCapture(PlayerHandle.Id, out InventorySnapshot? currentInventory)
        || currentInventory is null
        || !IsInventorySnapshotValid(
          currentInventory, PlayerHandle.Id, InventorySlotLayout.ArmorStart + 10)
        || currentInventory.Revision != SourceRevision.Inventory
        || _equipmentMetadata.Revision != SourceRevision.EquipmentMetadata
        || _world.Revision != SourceRevision.World)
      {
        return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
          "The captured Player, inventory, item metadata or world facts changed.");
      }

      OwnerCommitResult world = _world.Validate();
      return world.Status == OwnerCommitStatus.Applied
        ? OwnerCommitResult.Applied(PlayerVersion)
        : world;
    }
  }

  private sealed class SpatialFactsCapture(CapturedPlayerFacts inputs) : IPlayerSpatialFactsCapture
  {
    public SessionEpoch Epoch => inputs.Epoch;

    public EntityHandle PlayerHandle => inputs.PlayerHandle;

    public CommitVersion PlayerVersion => inputs.PlayerVersion;

    public PlayerSourceRevision SourceRevision => inputs.SourceRevision;

    public PlayerSpatialSourceFacts Facts => inputs.Calculation.Spatial;

    public OwnerCommitResult Validate() => inputs.Validate();
  }

  private sealed class PickupFactsCapture(CapturedPlayerFacts inputs) : IPlayerPickupFactsCapture
  {
    public SessionEpoch Epoch => inputs.Epoch;

    public EntityHandle PlayerHandle => inputs.PlayerHandle;

    public CommitVersion PlayerVersion => inputs.PlayerVersion;

    public PlayerSourceRevision SourceRevision => inputs.SourceRevision;

    public PlayerPickupSourceFacts Facts => inputs.Calculation.Pickup;

    public OwnerCommitResult Validate() => inputs.Validate();
  }

  private sealed class RuntimeTickWorldFacts(IPlayerRuntimeWorldCapture world)
    : IPlayerTickWorldFacts
  {
    public SessionEpoch Epoch => world.Epoch;

    public MinimalImplementation.Server.Identity.EntityHandle PlayerHandle => world.PlayerHandle;

    public CommitVersion PlayerVersion => world.PlayerVersion;

    public CommitVersion WorldRevision => world.Revision;

    public bool LanternsUp => world.LanternsUp;

    public int DayRate => world.DayRate;

    public OwnerCommitResult Validate() => world.Validate();
  }

}
