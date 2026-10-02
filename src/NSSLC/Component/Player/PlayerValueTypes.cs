namespace Terraria.Player;

public readonly record struct ItemEntityRef(Guid EntityId)
{
  public static ItemEntityRef None => new(Guid.Empty);

  public bool IsEmpty => EntityId == Guid.Empty;
}

public readonly record struct LegacyPlayerSlot(int Value);

public readonly record struct LegacyProjectileSlot(int Value);

public readonly record struct TileCoordinate(int X, int Y);

public readonly record struct WorldPosition(float X, float Y);

public readonly record struct SimulationTick(long Value);

public readonly record struct ContentId<TDefinition>(int Value);

public enum Direction : sbyte
{
  Left = -1,
  Right = 1,
}

public enum PlayerDifficulty : byte
{
  Classic,
  Mediumcore,
  Hardcore,
}

public enum PlayerConnectionState : byte
{
  Inactive,
  Active,
  Disconnecting,
}

public enum PlayerLifecyclePhase : byte
{
  Alive,
  Dead,
  Respawning,
  Spectating,
}

[Flags]
public enum PlayerRestActivity : byte
{
  None = 0,
  Petting = 1 << 0,
  Sitting = 1 << 1,
  Sleeping = 1 << 2,
}

public enum ItemUseMode : byte
{
  None,
  Primary,
  Alternate,
}

[Flags]
public enum RestSeatFeatures : byte
{
  None = 0,
  Toilet = 1,
}

[Flags]
public enum MinionFeatureFlags : ulong
{
  None = 0,
  Pygmy = 1UL << 0,
  PalworldFoxsparks = 1UL << 1,
}

public sealed class ItemContainerState
{
  private readonly ItemEntityRef[] _slots;

  public ItemContainerState(IReadOnlyList<ItemEntityRef> slots)
  {
    _slots = slots.ToArray();
  }

  public IReadOnlyList<ItemEntityRef> Slots => _slots;

  public int Capacity => _slots.Length;
}

public readonly record struct BuffSlot(ContentId<BuffDefinition> Type, int RemainingTicks)
{
  public bool IsEmpty => RemainingTicks <= 0;
}

public sealed class BuffDefinition
{
}

public sealed class ProjectileDefinition
{
}

public readonly record struct EquipmentLoadoutState(
  IReadOnlyList<ItemEntityRef> Equipment,
  IReadOnlyList<ItemEntityRef> Dyes,
  IReadOnlyList<bool> HiddenAccessories);

public readonly record struct EquipmentLoadoutView(
  IReadOnlyList<ItemEntityRef> Equipment,
  IReadOnlyList<ItemEntityRef> Dyes,
  IReadOnlyList<bool> HiddenAccessories);

public readonly record struct VoidVaultState(bool IsAvailable, bool IsOpen);
