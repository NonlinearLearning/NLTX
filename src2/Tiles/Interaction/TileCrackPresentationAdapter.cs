namespace Terraria.Tiles.Interaction;

public sealed class TileCrackPresentationAdapter
{
  private readonly ITileCrackStyleRandom _random;
  private int _lastCrackStyle = -1;

  public TileCrackPresentationAdapter(ITileCrackStyleRandom random)
  {
    _random = random ?? throw new ArgumentNullException(nameof(random));
  }

  public int SelectCrackStyle()
  {
    for (int attempt = 0; attempt < TileHitTrackingPolicy.CrackStyleCount; attempt++)
    {
      int candidate = _random.Next(TileHitTrackingPolicy.CrackStyleCount);
      if (candidate >= 0 && candidate < TileHitTrackingPolicy.CrackStyleCount &&
        candidate != _lastCrackStyle)
      {
        _lastCrackStyle = candidate;
        return candidate;
      }
    }

    _lastCrackStyle = _lastCrackStyle < 0 ? 0 :
      (_lastCrackStyle + 1) % TileHitTrackingPolicy.CrackStyleCount;
    return _lastCrackStyle;
  }
}
