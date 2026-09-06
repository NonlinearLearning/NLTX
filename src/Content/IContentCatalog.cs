namespace Terraria.Content;

public interface IContentCatalog
{
  long CatalogRevision { get; }

  IItemDefinitionQuery Items { get; }

  INpcDefinitionQuery Npcs { get; }

  IProjectileDefinitionQuery Projectiles { get; }

  IBuffDefinitionQuery Buffs { get; }

  ITileDefinitionQuery Tiles { get; }

  IWallDefinitionQuery Walls { get; }

  RecipeDefinitionCatalog Recipes { get; }

  RecipeGroupCatalog RecipeGroups { get; }

  DropRuleCatalog DropRules { get; }

  FishingDropRuleCatalog FishingDropRules { get; }

  IContentIdentityQuery Identities { get; }
}
