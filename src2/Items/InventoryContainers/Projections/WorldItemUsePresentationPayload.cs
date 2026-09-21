namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemUsePresentationPayload
{
  public WorldItemUsePresentationPayload(WorldItemUsePresentationSource source)
  {
    ArgumentNullException.ThrowIfNull(source);
    NewAndShiny = source.NewAndShiny;
    Color = source.Color;
    MakeNpc = source.MakeNpc;
    UseTime = source.UseTime;
    UseAnimation = source.UseAnimation;
    UseAmmo = source.UseAmmo;
    Damage = source.Damage;
    KnockBack = source.KnockBack;
    ShootSpeed = source.ShootSpeed;
    Scale = source.Scale;
    Ammo = source.Ammo;
    NotAmmo = source.NotAmmo;
    Shoot = source.Shoot;
    PlaceStyle = source.PlaceStyle;
    CreateTile = source.CreateTile;
    GlowMask = source.GlowMask;
    Expert = source.Expert;
    Alpha = source.Alpha;
    BuffType = source.BuffType;
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
