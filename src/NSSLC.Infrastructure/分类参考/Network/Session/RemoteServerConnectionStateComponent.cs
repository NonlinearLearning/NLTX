namespace Terraria.Network.Session;

public sealed class RemoteServerConnectionStateComponent
{
  public RemoteServerConnectionStateComponent(
    bool isActive = false,
    int state = 0,
    int timeOutTimer = 0,
    bool pendingTermination = false,
    bool isReading = false,
    byte serverSpecialFlags = 0)
  {
    if (isReading && !isActive)
    {
      throw new ArgumentException(
        "A remote server cannot be reading while inactive.",
        nameof(isReading));
    }

    IsActive = isActive;
    State = ValidateNonNegative(state, nameof(state));
    TimeOutTimer = ValidateNonNegative(timeOutTimer, nameof(timeOutTimer));
    PendingTermination = pendingTermination;
    IsReading = isReading;
    ServerSpecialFlags = serverSpecialFlags;
  }

  public bool IsActive { get; }

  public int State { get; }

  public int TimeOutTimer { get; }

  public bool PendingTermination { get; }

  public bool IsReading { get; }

  public byte ServerSpecialFlags { get; }

  private static int ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return value;
  }
}
