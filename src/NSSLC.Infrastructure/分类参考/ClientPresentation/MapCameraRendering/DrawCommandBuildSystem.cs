using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class DrawCommandBuildSystem
{
  public uint BeginFrame(DrawCommandWorksetComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.BeginFrame();
  }

  public DrawCommand Build(
    DrawCommandWorksetComponent component,
    uint frameLease,
    DrawCommandInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    Validate(input);
    DrawCommand command = new(
      input.ResourceToken,
      input.Position,
      input.DestinationRectangle,
      input.SourceRectangle,
      input.Color,
      input.Rotation,
      input.Origin,
      input.Scale,
      input.ShaderHandle,
      input.IgnorePlayerRotation,
      input.UseDestinationRectangle,
      input.NullRectangle,
      frameLease);
    component.Add(command);
    return command;
  }

  private static void Validate(DrawCommandInput input)
  {
    if (string.IsNullOrWhiteSpace(input.ResourceToken))
    {
      throw new ArgumentException("A draw resource token is required.", nameof(input));
    }

    if (!float.IsFinite(input.Rotation) ||
      !float.IsFinite(input.Position.X) ||
      !float.IsFinite(input.Position.Y) ||
      !float.IsFinite(input.Origin.X) ||
      !float.IsFinite(input.Origin.Y) ||
      !float.IsFinite(input.Scale.X) ||
      !float.IsFinite(input.Scale.Y) ||
      input.Scale.X <= 0f ||
      input.Scale.Y <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(input));
    }

    if (input.ShaderHandle < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(input.ShaderHandle));
    }

    if (input.UseDestinationRectangle && input.DestinationRectangle is null)
    {
      throw new ArgumentException(
        "Destination mode requires a destination rectangle.",
        nameof(input));
    }
  }
}
