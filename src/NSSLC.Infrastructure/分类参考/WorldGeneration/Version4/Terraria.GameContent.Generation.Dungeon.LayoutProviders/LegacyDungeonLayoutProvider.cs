using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation.Dungeon.Halls;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.LayoutProviders;

public class LegacyDungeonLayoutProvider : DungeonLayoutProvider
{
	public LegacyDungeonLayoutProvider(DungeonLayoutProviderSettings settings)
		: base(settings)
	{
	}

	public override void ProvideLayout(DungeonData data, GenerationProgress progress, UnifiedRandom
  genRand, ref int roomDelay){}
}
