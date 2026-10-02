namespace Terraria.Items;

public sealed class ContainerAccessComponent
{
  public ContainerAccessComponent(
    PersistentContainerId persistentContainerId,
    TileCoordinates? tilePosition = null,
    bool isLocked = false,
    ulong requiredAccessFlags = 0,
    RuntimeEntityId? currentAccessor = null,
    long? accessLeaseUntilTick = null,
    bool isShared = true)
  {
    PersistentContainerId = persistentContainerId;
    TilePosition = tilePosition;
    IsLocked = isLocked;
    RequiredAccessFlags = requiredAccessFlags;
    CurrentAccessor = currentAccessor;
    AccessLeaseUntilTick = accessLeaseUntilTick;
    IsShared = isShared;
  }

  public PersistentContainerId PersistentContainerId;
  public TileCoordinates? TilePosition;
  public bool IsLocked;
  public ulong RequiredAccessFlags;
  public RuntimeEntityId? CurrentAccessor;
  public long? AccessLeaseUntilTick;
  public bool IsShared;

  public bool HasActiveAccessorAt(long currentTick) =>
    CurrentAccessor.HasValue &&
    AccessLeaseUntilTick.HasValue &&
    currentTick < AccessLeaseUntilTick.Value;
}
