namespace Terraria.Items.InventoryContainers;

public sealed class ItemToolPlacementCapabilityComponent
{
  public ItemToolPlacementCapabilityComponent(
    int pick,
    int axe,
    int hammer,
    int tileBoost,
    int createTile,
    int createWall,
    int placeStyle,
    int ammo,
    bool notAmmo,
    int useAmmo,
    bool material)
  {
    Pick = pick;
    Axe = axe;
    Hammer = hammer;
    TileBoost = tileBoost;
    CreateTile = createTile;
    CreateWall = createWall;
    PlaceStyle = placeStyle;
    Ammo = ammo;
    NotAmmo = notAmmo;
    UseAmmo = useAmmo;
    Material = material;
  }

  public int Pick { get; }
  public int Axe { get; }
  public int Hammer { get; }
  public int TileBoost { get; }
  public int CreateTile { get; }
  public int CreateWall { get; }
  public int PlaceStyle { get; }
  public int Ammo { get; }
  public bool NotAmmo { get; }
  public int UseAmmo { get; }
  public bool Material { get; }
}
