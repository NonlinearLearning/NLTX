namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Version4 content facts needed by projectile synchronization and lifecycle rules.</summary>
public sealed record ProjectileDefinition
{
  public ProjectileDefinition(
    short type,
    int aiStyle,
    int width,
    int height,
    int defaultTimeLeft,
    bool friendly,
    bool hostile,
    bool netImportant,
    bool needsProtocolUuid,
    bool tileCollide,
    short penetrate,
    ProjectileAiProfile? aiProfile = null,
    bool minion = false,
    bool whip = false)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(type);
    ArgumentOutOfRangeException.ThrowIfNegative(aiStyle);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    ArgumentOutOfRangeException.ThrowIfNegative(defaultTimeLeft);
    if (penetrate == 0 || penetrate < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(penetrate));
    }

    Type = type;
    AiStyle = aiStyle;
    Width = width;
    Height = height;
    DefaultTimeLeft = defaultTimeLeft;
    Friendly = friendly;
    Hostile = hostile;
    NetImportant = netImportant;
    NeedsProtocolUuid = needsProtocolUuid;
    TileCollide = tileCollide;
    Penetrate = penetrate;
    AiProfile = aiProfile ?? ProjectileAiProfileCatalog.Resolve(aiStyle);
    Minion = minion;
    Whip = whip;
  }

  public short Type { get; }

  public int AiStyle { get; }

  public int Width { get; }

  public int Height { get; }

  public int DefaultTimeLeft { get; }

  public bool Friendly { get; }

  public bool Hostile { get; }

  public bool NetImportant { get; }

  public bool NeedsProtocolUuid { get; }

  public bool TileCollide { get; }

  public short Penetrate { get; }

  public ProjectileAiProfile AiProfile { get; }

  public bool Minion { get; }

  public bool Whip { get; }
}
