namespace Terraria.Network.Session;

public sealed class NetworkSessionModeStateComponent
{
  private NetworkSessionMode _currentMode;
  private NetworkSessionMode _targetMode;
  private bool _hasPendingTransition;
  private int _lastItemUpdate;

  public NetworkSessionModeStateComponent(
    NetworkSessionMode initialMode,
    int maxItemUpdates)
  {
    ValidateMode(initialMode);
    if (maxItemUpdates < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxItemUpdates));
    }

    _currentMode = initialMode;
    _targetMode = initialMode;
    MaxItemUpdates = maxItemUpdates;
  }

  public int MaxItemUpdates { get; }

  public bool RequestTransition(NetworkModeChangeCommand command)
  {
    if (!IsDefined(command.TargetMode))
    {
      return false;
    }

    _targetMode = command.TargetMode;
    _hasPendingTransition = true;
    return true;
  }

  public bool CommitTransition()
  {
    if (!_hasPendingTransition)
    {
      return false;
    }

    _currentMode = _targetMode;
    _hasPendingTransition = false;
    return true;
  }

  public void RecordItemUpdate(int itemUpdate)
  {
    if (itemUpdate < 0 || itemUpdate > MaxItemUpdates)
    {
      throw new ArgumentOutOfRangeException(nameof(itemUpdate));
    }

    _lastItemUpdate = itemUpdate;
  }

  public NetworkSessionModeSnapshot CreateSnapshot()
  {
    return new NetworkSessionModeSnapshot(
      _currentMode,
      _targetMode,
      _hasPendingTransition,
      GetIp: _currentMode != NetworkSessionMode.SinglePlayerClient,
      MenuMultiplayer: _targetMode == NetworkSessionMode.MultiplayerClient,
      MenuServer: _targetMode == NetworkSessionMode.Server,
      NetPlayCounter: _hasPendingTransition ? 1 : 0,
      _lastItemUpdate,
      MaxItemUpdates);
  }

  private static bool IsDefined(NetworkSessionMode mode)
  {
    return mode is NetworkSessionMode.SinglePlayerClient or
      NetworkSessionMode.MultiplayerClient or
      NetworkSessionMode.Server;
  }

  private static void ValidateMode(NetworkSessionMode mode)
  {
    if (!IsDefined(mode))
    {
      throw new ArgumentOutOfRangeException(nameof(mode));
    }
  }
}
