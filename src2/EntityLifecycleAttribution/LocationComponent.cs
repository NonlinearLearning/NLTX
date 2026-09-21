using System.Numerics;

namespace Terraria.EntityLifecycleAttribution;

public sealed class LocationComponent
{
  public LocationComponent(Vector2 position)
  {
    Position = position;
  }

  public Vector2 Position { get; private set; }

  public void SetPosition(Vector2 position)
  {
    Position = position;
  }
}
