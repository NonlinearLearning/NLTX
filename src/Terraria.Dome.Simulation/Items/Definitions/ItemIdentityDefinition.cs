namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemIdentityDefinition(
  string NameKey = "",
  bool IsMaterial = false,
  bool IsQuestItem = false,
  bool UniqueStack = false,
  bool ExpertOnly = false,
  bool IsShopCurrency = false);
