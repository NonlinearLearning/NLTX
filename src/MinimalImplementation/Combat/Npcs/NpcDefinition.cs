namespace MinimalImplementation.Combat.Npcs;

/// <summary>Version4 content facts required to create and synchronize one NPC type.</summary>
public sealed record NpcDefinition
{
  public NpcDefinition(
    short type,
    int aiStyle,
    int width,
    int height,
    int maximumLife,
    bool isBoss,
    bool isTownNpc,
    bool netAlways,
    bool catchable,
    NpcAiProfile? aiProfile = null,
    int damage = 0,
    int defense = 0,
    bool friendly = false,
    bool dontTakeDamage = false,
float knockbackResist = 1f,
    int? clientMaximumLife = null)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(type);
    ArgumentOutOfRangeException.ThrowIfNegative(aiStyle);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumLife);
    ArgumentOutOfRangeException.ThrowIfNegative(damage);
    ArgumentOutOfRangeException.ThrowIfNegative(defense);
    if (!float.IsFinite(knockbackResist) || knockbackResist < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(knockbackResist));
    }

    Type = type;
    AiStyle = aiStyle;
    Width = width;
    Height = height;
    MaximumLife = maximumLife;
    ClientMaximumLife = clientMaximumLife ?? maximumLife;
    IsBoss = isBoss;
    IsTownNpc = isTownNpc;
    NetAlways = netAlways;
    Catchable = catchable;
    AiProfile = aiProfile ?? NpcAiProfileCatalog.Resolve(aiStyle);
    Damage = damage;
    Defense = defense;
    Friendly = friendly;
    DontTakeDamage = dontTakeDamage;
    KnockbackResist = knockbackResist;
  }

  public short Type { get; }

  public int AiStyle { get; }

  public int Width { get; }

  public int Height { get; }

  public int MaximumLife { get; }

  public int ClientMaximumLife { get; }

  internal NpcDefinition WithMaximumLife(int maximumLife) {
    return new NpcDefinition(Type, AiStyle, Width, Height, maximumLife, IsBoss,
        IsTownNpc, NetAlways, Catchable, AiProfile, Damage, Defense, Friendly,
        DontTakeDamage, KnockbackResist, ClientMaximumLife);
  }

  public bool IsBoss { get; }

  public bool IsTownNpc { get; }

  public bool NetAlways { get; }

  public bool Catchable { get; }

  public NpcAiProfile AiProfile { get; }

  public int Damage { get; }

  public int Defense { get; }

  public bool Friendly { get; }

  public bool DontTakeDamage { get; }

  public float KnockbackResist { get; }
}
