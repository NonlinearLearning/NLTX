using System.Numerics;

namespace Terraria.Player.Mount;

public sealed class MountSpecialVehicleCatalogDefinition
{
  private readonly IReadOnlyList<Vector2> _scutlixEyePositions;

  public MountSpecialVehicleCatalogDefinition(
    Vector2 scutlixTextureSize,
    IReadOnlyList<Vector2> scutlixEyePositions)
    : this(
      scutlixTextureSize,
      scutlixEyePositions,
      scutlixBaseDamage: 50,
      santankTextureSize: new Vector2(23f, 2f))
  {
  }

  public MountSpecialVehicleCatalogDefinition(
    Vector2 scutlixTextureSize,
    IReadOnlyList<Vector2> scutlixEyePositions,
    int scutlixBaseDamage,
    Vector2 santankTextureSize)
  {
    ArgumentNullException.ThrowIfNull(scutlixEyePositions);
    if (scutlixEyePositions.Count == 0)
    {
      throw new ArgumentException(
        "At least one Scutlix eye position is required.",
        nameof(scutlixEyePositions));
    }

    if (scutlixBaseDamage <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(scutlixBaseDamage));
    }

    if (santankTextureSize.X < 0f || santankTextureSize.Y < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(santankTextureSize));
    }

    ScutlixTextureSize = scutlixTextureSize;
    ScutlixBaseDamage = scutlixBaseDamage;
    SantankTextureSize = santankTextureSize;
    _scutlixEyePositions = Array.AsReadOnly(scutlixEyePositions.ToArray());
  }

  public Vector2 ScutlixTextureSize { get; }

  public int ScutlixBaseDamage { get; }

  public Vector2 SantankTextureSize { get; }

  public IReadOnlyList<Vector2> ScutlixEyePositions => _scutlixEyePositions;

  public Vector2 GetScutlixEyePosition(int index)
  {
    return _scutlixEyePositions[index];
  }

  public static MountSpecialVehicleCatalogDefinition Version4 { get; } =
    new(
      new Vector2(45f, 54f),
      new[]
      {
        new Vector2(15f, -52f),
        new Vector2(25f, -48f),
        new Vector2(23f, -48f),
        new Vector2(31f, -42f),
        new Vector2(35f, -44f),
        new Vector2(39f, -36f),
        new Vector2(29f, -34f),
        new Vector2(31f, -30f),
        new Vector2(25f, -20f),
        new Vector2(31f, -20f),
      },
      scutlixBaseDamage: 50,
      santankTextureSize: new Vector2(23f, 2f));
}
