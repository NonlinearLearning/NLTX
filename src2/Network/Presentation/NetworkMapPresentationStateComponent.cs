namespace Terraria.Network.Presentation;

public sealed class NetworkMapPresentationStateComponent
{
  private int _instantTransitionCounter;
  private int _backgroundDelay;
  private int _backgroundStyle;
  private float _frontLayerAlpha;
  private float _farBackLayerAlpha;
  private int _wallOfFleshNpcIndex = -1;
  private int _drawAreaTop;
  private int _drawAreaBottom;
  private bool _refreshMap;
  private bool _mapReady;
  private bool _updateMap;
  private int _mapTimeMax;
  private int _mapTime;
  private bool _clearMap;

  public void ApplyBackground(NetworkBackgroundPresentationCommand command)
  {
    if (command.BackgroundDelay < 0 ||
        command.WallOfFleshNpcIndex < -1 ||
        command.DrawAreaBottom < command.DrawAreaTop ||
        command.FrontLayerAlpha is < 0 or > 1 ||
        command.FarBackLayerAlpha is < 0 or > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    _instantTransitionCounter = command.InstantTransitionCounter;
    _backgroundDelay = command.BackgroundDelay;
    _backgroundStyle = command.BackgroundStyle;
    _frontLayerAlpha = command.FrontLayerAlpha;
    _farBackLayerAlpha = command.FarBackLayerAlpha;
    _wallOfFleshNpcIndex = command.WallOfFleshNpcIndex;
    _drawAreaTop = command.DrawAreaTop;
    _drawAreaBottom = command.DrawAreaBottom;
  }

  public void ApplyMapRefresh(MapRefreshCommand command)
  {
    if (command.MapTimeMax < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    _mapTimeMax = command.MapTimeMax;
    _mapTime = 0;
    _refreshMap = true;
    _mapReady = false;
    _updateMap = command.UpdateMap;
    _clearMap = command.ClearMap;
  }

  public void AdvanceMap(int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    if (!_refreshMap)
    {
      return;
    }

    _mapTime = Math.Min(_mapTimeMax, checked(_mapTime + ticks));
    if (_mapTime >= _mapTimeMax)
    {
      _refreshMap = false;
      _mapReady = true;
      _clearMap = false;
    }
  }

  public NetworkMapPresentationSnapshot CreateSnapshot()
  {
    return new NetworkMapPresentationSnapshot(
      _instantTransitionCounter,
      _backgroundDelay,
      _backgroundStyle,
      _frontLayerAlpha,
      _farBackLayerAlpha,
      _wallOfFleshNpcIndex,
      _drawAreaTop,
      _drawAreaBottom,
      _refreshMap,
      _mapReady,
      _updateMap,
      _mapTimeMax,
      _mapTime,
      _clearMap);
  }
}
