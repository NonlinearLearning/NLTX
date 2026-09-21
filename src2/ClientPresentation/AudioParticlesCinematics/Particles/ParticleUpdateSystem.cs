using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class ParticleUpdateSystem
{
  public void Update(
    StarVisualsComponent stars,
    RainVisualsComponent rain,
    float deltaSeconds,
    float worldBottom)
  {
    ArgumentNullException.ThrowIfNull(stars);
    ArgumentNullException.ThrowIfNull(rain);
    ValidateDelta(deltaSeconds);

    foreach (KeyValuePair<int, StarVisualState> entry in stars.Entries)
    {
      StarVisualState state = entry.Value;
      Vector2 velocity = state.Velocity;
      if (state.Falling)
      {
        velocity += state.FallSpeed;
      }

      stars.Set(entry.Key, state with
      {
        Position = state.Position + velocity * deltaSeconds,
        Rotation = state.Rotation + state.RotationSpeed * deltaSeconds,
        FallTime = state.FallTime + (state.Falling ? 1 : 0)
      });
    }

    foreach (KeyValuePair<int, RainVisualState> entry in rain.Entries)
    {
      RainVisualState state = entry.Value;
      Vector2 position = state.Position + state.Velocity * deltaSeconds;
      if (state.Kill || !state.Active || position.Y > worldBottom)
      {
        rain.Remove(entry.Key);
        continue;
      }

      rain.Set(entry.Key, state with { Position = position });
    }
  }

  public void Update(CloudVisualsComponent clouds, float deltaSeconds)
  {
    ArgumentNullException.ThrowIfNull(clouds);
    ValidateDelta(deltaSeconds);

    foreach (KeyValuePair<int, CloudVisualState> entry in clouds.Entries)
    {
      CloudVisualState state = entry.Value;
      if (state.Kill || !state.Active)
      {
        clouds.Remove(entry.Key);
        continue;
      }

      clouds.Set(entry.Key, state with
      {
        Rotation = state.Rotation + state.RotationSpeed * deltaSeconds,
        Scale = MathF.Max(0, state.Scale + state.ScaleSpeed * deltaSeconds)
      });
    }
  }

  public void Update(DustVisualsComponent dust, float deltaSeconds)
  {
    ArgumentNullException.ThrowIfNull(dust);
    ValidateDelta(deltaSeconds);

    foreach (KeyValuePair<int, DustVisualState> entry in dust.Entries)
    {
      DustVisualState state = entry.Value;
      if (!state.Active)
      {
        dust.Remove(entry.Key);
        continue;
      }

      dust.Set(entry.Key, state with
      {
        Position = state.Position + state.Velocity * deltaSeconds,
        Rotation = state.Rotation + deltaSeconds
      });
    }
  }

  public void Update(GoreVisualsComponent gore, float deltaSeconds)
  {
    ArgumentNullException.ThrowIfNull(gore);
    ValidateDelta(deltaSeconds);

    foreach (KeyValuePair<int, GoreVisualState> entry in gore.Entries)
    {
      GoreVisualState state = entry.Value;
      int timeLeft = state.TimeLeft - 1;
      if (!state.Active || timeLeft <= 0)
      {
        gore.Remove(entry.Key);
        continue;
      }

      gore.Set(entry.Key, state with
      {
        Position = state.Position + state.Velocity * deltaSeconds,
        Rotation = state.Rotation + deltaSeconds,
        TimeLeft = timeLeft,
        GoreTime = state.GoreTime + 1
      });
    }
  }

  private static void ValidateDelta(float deltaSeconds)
  {
    if (float.IsNaN(deltaSeconds) ||
      float.IsInfinity(deltaSeconds) ||
      deltaSeconds < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
    }
  }
}
