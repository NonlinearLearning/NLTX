namespace Terraria.Network.Session;

public sealed class RemoteClientIdentityAndLifecycleStateComponent
{
  public RemoteClientIdentityAndLifecycleStateComponent(
    int compatibilitySlot,
    string name = "Anonymous",
    int lifecycleState = 0,
    int timeoutTicks = 0,
    bool isActive = false,
    bool pendingTermination = false,
    bool pendingTerminationApproved = false,
    bool isAnnouncementCompleted = false)
  {
    if (compatibilitySlot < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(compatibilitySlot));
    }

    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("Client name is required.", nameof(name));
    }

    if (lifecycleState < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lifecycleState));
    }

    if (timeoutTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(timeoutTicks));
    }

    CompatibilitySlot = compatibilitySlot;
    Name = name;
    LifecycleState = lifecycleState;
    TimeoutTicks = timeoutTicks;
    IsActive = isActive;
    PendingTermination = pendingTermination;
    PendingTerminationApproved = pendingTerminationApproved;
    IsAnnouncementCompleted = isAnnouncementCompleted;
  }

  public int CompatibilitySlot { get; }

  public bool IsActive { get; private set; }

  public bool IsAnnouncementCompleted { get; private set; }

  public int LifecycleState { get; }

  public string Name { get; }

  public bool PendingTermination { get; private set; }

  public bool PendingTerminationApproved { get; private set; }

  public int TimeoutTicks { get; }
}
