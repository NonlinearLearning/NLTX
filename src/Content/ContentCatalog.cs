namespace Terraria.Content;

public sealed class ContentCatalog : IContentCatalog
{
  public ContentCatalog(ContentCatalogSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.IsValidated)
      throw new InvalidOperationException("Only validated content snapshots can be published.");
    Snapshot = snapshot;
  }

  public ContentCatalogSnapshot Snapshot { get; }

  public long CatalogRevision => Snapshot.CatalogRevision;

  public IItemDefinitionQuery Items => Snapshot.Items;

  public INpcDefinitionQuery Npcs => Snapshot.Npcs;

  public IProjectileDefinitionQuery Projectiles => Snapshot.Projectiles;

  public IBuffDefinitionQuery Buffs => Snapshot.Buffs;

  public ITileDefinitionQuery Tiles => Snapshot.Tiles;

  public IWallDefinitionQuery Walls => Snapshot.Walls;

  public RecipeDefinitionCatalog Recipes => Snapshot.Recipes;

  public RecipeGroupCatalog RecipeGroups => Snapshot.RecipeGroups;

  public DropRuleCatalog DropRules => Snapshot.DropRules;

  public FishingDropRuleCatalog FishingDropRules => Snapshot.FishingDropRules;

  public ContentDerivedIndexCatalog DerivedIndexes => Snapshot.DerivedIndexes;

  public ContentPresentationIndex PresentationIndexes => Snapshot.PresentationIndexes;

  public IContentIdentityQuery Identities => Snapshot.Identities;
}
