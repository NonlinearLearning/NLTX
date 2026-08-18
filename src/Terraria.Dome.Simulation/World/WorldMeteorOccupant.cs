namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldMeteorOccupant(
  float X,
  float Y,
  float Width,
  float Height)
{
  public bool Intersects(int impactX, int impactY, int impactRadius)
  {
    float left = X;
    float right = X + Width;
    float top = Y;
    float bottom = Y + Height;
    return right >= impactX - impactRadius && left <= impactX + impactRadius &&
      bottom >= impactY - impactRadius && top <= impactY + impactRadius;
  }
}
