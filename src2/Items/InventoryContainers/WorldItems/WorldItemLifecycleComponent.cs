namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemLifecycleComponent
{
  public const int UnreservedPlayerIndex = 255;

  public int OwnTime { get; private set; }

  public int KeepTime { get; private set; }

  public float ShimmerTime { get; private set; }

  public bool Shimmered { get; private set; }

  public bool Instanced { get; private set; }

  public bool BeingGrabbed { get; private set; }

  public bool OnConveyor { get; private set; }

  public int TimeSinceSpawned { get; private set; }

  public int TimeSinceTheItemHasBeenReservedForSomeone { get; private set; }

  public int NoGrabDelay { get; private set; }

  public int TimeLeftInWhichTheItemCannotBeTakenByEnemies { get; private set; }

  public int PlayerIndexTheItemIsReservedFor { get; private set; } = UnreservedPlayerIndex;

  public int OwnIgnore { get; private set; } = -1;

  public void Initialize(int ownTime, int keepTime)
  {
    if (ownTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ownTime));
    }

    if (keepTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(keepTime));
    }

    OwnTime = ownTime;
    KeepTime = keepTime;
    TimeSinceSpawned = 0;
    TimeSinceTheItemHasBeenReservedForSomeone = 0;
    PlayerIndexTheItemIsReservedFor = UnreservedPlayerIndex;
    OwnIgnore = -1;
    NoGrabDelay = 0;
    TimeLeftInWhichTheItemCannotBeTakenByEnemies = 0;
    ShimmerTime = 0;
    Shimmered = false;
    Instanced = false;
    BeingGrabbed = false;
    OnConveyor = false;
  }

  public void Advance(WorldItemLifecycleTick tick)
  {
    if (!tick.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    OwnTime = Math.Max(0, OwnTime - tick.ElapsedTicks);
    KeepTime = Math.Max(0, KeepTime - tick.ElapsedTicks);
    NoGrabDelay = Math.Max(0, NoGrabDelay - tick.ElapsedTicks);
    TimeLeftInWhichTheItemCannotBeTakenByEnemies = Math.Max(
      0,
      TimeLeftInWhichTheItemCannotBeTakenByEnemies - tick.ElapsedTicks);
    TimeSinceSpawned = Math.Min(int.MaxValue, TimeSinceSpawned + tick.ElapsedTicks);
    if (PlayerIndexTheItemIsReservedFor == UnreservedPlayerIndex)
    {
      TimeSinceTheItemHasBeenReservedForSomeone = 0;
    }
    else
    {
      TimeSinceTheItemHasBeenReservedForSomeone = Math.Min(
        int.MaxValue,
        TimeSinceTheItemHasBeenReservedForSomeone + tick.ElapsedTicks);
    }
    ShimmerTime += tick.DeltaSeconds;
  }

  public void ReserveForPlayer(int playerIndex)
  {
    if (playerIndex < 0 || playerIndex >= UnreservedPlayerIndex)
    {
      throw new ArgumentOutOfRangeException(nameof(playerIndex));
    }

    PlayerIndexTheItemIsReservedFor = playerIndex;
    TimeSinceTheItemHasBeenReservedForSomeone = 0;
  }

  public void ReleaseReservation()
  {
    PlayerIndexTheItemIsReservedFor = UnreservedPlayerIndex;
    TimeSinceTheItemHasBeenReservedForSomeone = 0;
  }

  public void SetOwnIgnore(int playerIndex)
  {
    if (playerIndex < -1 || playerIndex >= UnreservedPlayerIndex)
    {
      throw new ArgumentOutOfRangeException(nameof(playerIndex));
    }

    OwnIgnore = playerIndex;
  }

  public void SetNoGrabDelay(int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    NoGrabDelay = ticks;
  }

  public void SetEnemyPickupDelay(int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    TimeLeftInWhichTheItemCannotBeTakenByEnemies = ticks;
  }

  public void SetShimmered(bool shimmered)
  {
    Shimmered = shimmered;
  }

  public void SetInstanced(bool instanced)
  {
    Instanced = instanced;
  }

  public void SetBeingGrabbed(bool beingGrabbed)
  {
    BeingGrabbed = beingGrabbed;
  }

  public void SetOnConveyor(bool onConveyor)
  {
    OnConveyor = onConveyor;
  }
}
