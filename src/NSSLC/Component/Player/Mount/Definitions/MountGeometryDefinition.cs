namespace Terraria.Player.Mount;

public sealed class MountGeometryDefinition
{
  private readonly IReadOnlyList<int> _playerYOffsets;

  public MountGeometryDefinition(
    int textureWidth,
    int textureHeight,
    int xOffset,
    int yOffset,
    int bodyFrame,
    int playerHeadOffset,
    int heightBoost,
    int playerXOffset,
    IReadOnlyList<int> playerYOffsets)
  {
    ArgumentNullException.ThrowIfNull(playerYOffsets);
    if (textureWidth < 0 || textureHeight < 0 || heightBoost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(heightBoost));
    }

    TextureWidth = textureWidth;
    TextureHeight = textureHeight;
    XOffset = xOffset;
    YOffset = yOffset;
    BodyFrame = bodyFrame;
    PlayerHeadOffset = playerHeadOffset;
    HeightBoost = heightBoost;
    PlayerXOffset = playerXOffset;
    _playerYOffsets = Array.AsReadOnly(playerYOffsets.ToArray());
  }

  public int TextureWidth { get; }

  public int TextureHeight { get; }

  public int XOffset { get; }

  public int YOffset { get; }

  public int BodyFrame { get; }

  public int PlayerHeadOffset { get; }

  public int HeightBoost { get; }

  public int PlayerXOffset { get; }

  public IReadOnlyList<int> PlayerYOffsets => _playerYOffsets;
}
