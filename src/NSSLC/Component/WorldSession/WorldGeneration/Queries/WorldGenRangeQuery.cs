using System;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Resolves a world-generation range from explicit dimensions and a caller-owned random source.
/// </summary>
public static class WorldGenRangeQuery
{
  public enum ScalingMode : byte
  {
    None,
    WorldArea,
    WorldWidth,
  }

  public readonly record struct Input(
    int Minimum,
    int Maximum,
    ScalingMode ScaleWith,
    int WorldWidth,
    int WorldHeight,
    GenerationRandomStream RandomStream);

  public readonly record struct Result(
    int ScaledMinimum,
    int ScaledMaximum,
    int Value,
    GenerationRandomStream RandomStream);

  /// <summary>
  /// Applies the same inclusive range scaling used by the legacy WorldGenRange type.
  /// </summary>
  public static int ScaleValue(
    int value,
    ScalingMode scaleWith,
    int worldWidth,
    int worldHeight)
  {
    ValidateDimensions(worldWidth, worldHeight);
    if (!Enum.IsDefined(scaleWith))
    {
      throw new ArgumentOutOfRangeException(nameof(scaleWith));
    }

    double factor = scaleWith switch
    {
      ScalingMode.WorldArea =>
        ((double)worldWidth * worldHeight) / 5040000.0d,
      ScalingMode.WorldWidth => (double)worldWidth / 4200.0d,
      ScalingMode.None => 1.0d,
      _ => throw new ArgumentOutOfRangeException(nameof(scaleWith)),
    };

    return (int)(factor * value);
  }

  /// <summary>
  /// Chooses an inclusive random value without reading ambient world or random state.
  /// </summary>
  public static Result GetRandom(
    in Input input,
    IGenerationRandomSource random)
  {
    ArgumentNullException.ThrowIfNull(random);
    ValidateInput(input);

    int scaledMinimum = ScaleValue(
      input.Minimum,
      input.ScaleWith,
      input.WorldWidth,
      input.WorldHeight);
    int scaledMaximum = ScaleValue(
      input.Maximum,
      input.ScaleWith,
      input.WorldWidth,
      input.WorldHeight);
    if (scaledMaximum < scaledMinimum)
    {
      throw new ArgumentException(
        "The scaled maximum must not be less than the scaled minimum.",
        nameof(input));
    }

    if (scaledMaximum == int.MaxValue)
    {
      throw new OverflowException(
        "An inclusive maximum of Int32.MaxValue cannot be represented by the random port.");
    }

    int value = random.NextInt(
      input.RandomStream,
      scaledMinimum,
      scaledMaximum + 1);
    return new Result(
      scaledMinimum,
      scaledMaximum,
      value,
      input.RandomStream);
  }

  private static void ValidateInput(in Input input)
  {
    if (input.Minimum > input.Maximum)
    {
      throw new ArgumentException(
        "The range minimum must not exceed its maximum.",
        nameof(input));
    }

    ValidateDimensions(input.WorldWidth, input.WorldHeight);
    if (!Enum.IsDefined(input.ScaleWith))
    {
      throw new ArgumentOutOfRangeException(nameof(input.ScaleWith));
    }

    if (!Enum.IsDefined(input.RandomStream))
    {
      throw new ArgumentOutOfRangeException(nameof(input.RandomStream));
    }
  }

  private static void ValidateDimensions(int worldWidth, int worldHeight)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldHeight);
  }
}
