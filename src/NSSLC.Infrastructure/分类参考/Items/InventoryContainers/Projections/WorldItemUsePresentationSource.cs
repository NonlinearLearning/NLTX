namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemUsePresentationSource
{
  public WorldItemUsePresentationSource(
    bool newAndShiny,
    ItemPresentationColor color,
    short makeNpc,
    int useTime,
    int useAnimation,
    int useAmmo,
    int damage,
    float knockBack,
    float shootSpeed,
    float scale,
    int ammo,
    bool notAmmo,
    int shoot,
    int placeStyle,
    int createTile,
    int glowMask,
    bool expert,
    int alpha,
    int buffType)
  {
    NewAndShiny = newAndShiny;
    Color = color;
    MakeNpc = makeNpc;
    UseTime = useTime;
    UseAnimation = useAnimation;
    UseAmmo = useAmmo;
    Damage = damage;
    KnockBack = knockBack;
    ShootSpeed = shootSpeed;
    Scale = scale;
    Ammo = ammo;
    NotAmmo = notAmmo;
    Shoot = shoot;
    PlaceStyle = placeStyle;
    CreateTile = createTile;
    GlowMask = glowMask;
    Expert = expert;
    Alpha = alpha;
    BuffType = buffType;
  }

  public bool NewAndShiny { get; }
  public ItemPresentationColor Color { get; }
  public short MakeNpc { get; }
  public int UseTime { get; }
  public int UseAnimation { get; }
  public int UseAmmo { get; }
  public int Damage { get; }
  public float KnockBack { get; }
  public float ShootSpeed { get; }
  public float Scale { get; }
  public int Ammo { get; }
  public bool NotAmmo { get; }
  public int Shoot { get; }
  public int PlaceStyle { get; }
  public int CreateTile { get; }
  public int GlowMask { get; }
  public bool Expert { get; }
  public int Alpha { get; }
  public int BuffType { get; }
}
