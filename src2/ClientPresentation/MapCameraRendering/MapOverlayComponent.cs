using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapOverlayComponent
{
  private readonly List<MapPingProjection> _pings = new();
  private readonly List<MapPylonProjection> _pylons = new();

  public Vector2 Translation { get; private set; }

  public SceneScanRectangle ClipArea { get; private set; }

  public float Opacity { get; private set; }

  public int PingCount => _pings.Count;

  public int PylonCount => _pylons.Count;

  public IReadOnlyList<MapPingProjection> Pings => _pings.ToArray();

  public IReadOnlyList<MapPylonProjection> Pylons => _pylons.ToArray();

  public uint Revision { get; private set; }

  internal void ReplaceTransform(
    Vector2 translation,
    SceneScanRectangle clipArea,
    float opacity)
  {
    Translation = translation;
    ClipArea = clipArea.Normalize();
    Opacity = opacity;
    Revision++;
  }

  internal void AddPing(MapPingProjection ping)
  {
    if (ping.PingId == Guid.Empty)
    {
      throw new ArgumentException("A map ping requires an identity.", nameof(ping));
    }

    if (!float.IsFinite(ping.Position.X) || !float.IsFinite(ping.Position.Y) ||
      ping.ExpiresMilliseconds < ping.CreatedMilliseconds)
    {
      throw new ArgumentOutOfRangeException(nameof(ping));
    }

    _pings.RemoveAll(existing => existing.PingId == ping.PingId);
    _pings.Add(ping);
    Revision++;
  }

  internal void AddPylon(MapPylonProjection pylon)
  {
    if (pylon.PylonId == Guid.Empty)
    {
      throw new ArgumentException("A map pylon requires an identity.", nameof(pylon));
    }

    _pylons.RemoveAll(existing => existing.PylonId == pylon.PylonId);
    _pylons.Add(pylon);
    Revision++;
  }

  internal void ExpirePings(MapClockSnapshot clock)
  {
    int removed = _pings.RemoveAll(ping => ping.ExpiresMilliseconds <= clock.CurrentMilliseconds);
    if (removed > 0)
    {
      Revision++;
    }
  }
}
