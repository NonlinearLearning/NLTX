using NSSLC.WorldGeneration.Utilities;
using NSSLC.WorldGeneration.WorldBuilding;

namespace NSSLC.WorldGeneration.GameContent.Generation.Dungeon;

public abstract class DungeonLayoutProvider
{
	public DungeonLayoutProviderSettings settings;

	public DungeonLayoutProvider(DungeonLayoutProviderSettings settings)
	{
		this.settings = settings;
	}

	public abstract void ProvideLayout(DungeonData data, GenerationProgress progress, UnifiedRandom genRand, ref int roomDelay);
}
