namespace Terraria.Content;

public sealed class ContentCatalogSnapshot
{
  public ContentCatalogSnapshot(
    long catalogRevision,
    string sourceKey,
    ItemDefinitionCatalog items,
    NpcDefinitionCatalog npcs,
    ProjectileDefinitionCatalog projectiles,
    ContentIdentityCatalog identities,
    BuffDefinitionCatalog? buffs = null,
    TileDefinitionCatalog? tiles = null,
    WallDefinitionCatalog? walls = null,
    RecipeDefinitionCatalog? recipes = null,
    RecipeGroupCatalog? recipeGroups = null,
    DropRuleCatalog? dropRules = null,
    FishingDropRuleCatalog? fishingDropRules = null,
    bool isValidated = false)
  {
    if (isValidated)
      throw new ArgumentException(
        "Only the content catalog build system can create a validated snapshot.",
        nameof(isValidated));
    ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(projectiles);
    ArgumentNullException.ThrowIfNull(identities);
    CatalogRevision = catalogRevision;
    SourceKey = sourceKey;
    Items = items;
    Npcs = npcs;
    Projectiles = projectiles;
    Identities = identities;
    Buffs = buffs ?? new BuffDefinitionCatalog(Array.Empty<BuffDefinition>());
    Tiles = tiles ?? new TileDefinitionCatalog(Array.Empty<TileDefinition>());
    Walls = walls ?? new WallDefinitionCatalog(Array.Empty<WallDefinition>());
    Recipes = recipes ?? new RecipeDefinitionCatalog(Array.Empty<RecipeDefinition>());
    RecipeGroups = recipeGroups ?? new RecipeGroupCatalog(Array.Empty<RecipeGroupDefinition>());
    DropRules = dropRules ?? new DropRuleCatalog(
      Array.Empty<DropRuleCatalogEntry>(),
      new Dictionary<int, IReadOnlyList<DropRuleCatalogEntry>>());
    FishingDropRules = fishingDropRules ?? new FishingDropRuleCatalog(Array.Empty<FishingDropRuleDefinition>());
    DerivedIndexes = new ContentDerivedIndexCatalog(Items, Projectiles, Npcs);
    PresentationIndexes = new ContentPresentationIndex(Items, Npcs, Projectiles);
    IsValidated = false;
  }

  private ContentCatalogSnapshot(ContentCatalogSnapshot source, bool isValidated)
  {
    CatalogRevision = source.CatalogRevision;
    SourceKey = source.SourceKey;
    Items = source.Items;
    Npcs = source.Npcs;
    Projectiles = source.Projectiles;
    Identities = source.Identities;
    Buffs = source.Buffs;
    Tiles = source.Tiles;
    Walls = source.Walls;
    Recipes = source.Recipes;
    RecipeGroups = source.RecipeGroups;
    DropRules = source.DropRules;
    FishingDropRules = source.FishingDropRules;
    DerivedIndexes = source.DerivedIndexes;
    PresentationIndexes = source.PresentationIndexes;
    IsValidated = isValidated;
  }

  public long CatalogRevision { get; }

  public string SourceKey { get; }

  public ItemDefinitionCatalog Items { get; }

  public NpcDefinitionCatalog Npcs { get; }

  public ProjectileDefinitionCatalog Projectiles { get; }

  public ContentIdentityCatalog Identities { get; }

  public BuffDefinitionCatalog Buffs { get; }

  public TileDefinitionCatalog Tiles { get; }

  public WallDefinitionCatalog Walls { get; }

  public RecipeDefinitionCatalog Recipes { get; }

  public RecipeGroupCatalog RecipeGroups { get; }

  public DropRuleCatalog DropRules { get; }

  public FishingDropRuleCatalog FishingDropRules { get; }

  public bool IsValidated { get; }

  public ContentDerivedIndexCatalog DerivedIndexes { get; }

  public ContentPresentationIndex PresentationIndexes { get; }

  internal ContentCatalogSnapshot CreateValidated()
  {
    return new ContentCatalogSnapshot(this, true);
  }
}
