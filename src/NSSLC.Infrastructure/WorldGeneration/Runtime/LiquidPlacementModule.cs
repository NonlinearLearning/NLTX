using NSSLC.WorldGeneration.Enums;

namespace NSSLC.WorldGeneration.Modules;

public class LiquidPlacementModule
{
	public LiquidPlacement water;

	public LiquidPlacement lava;

	public LiquidPlacementModule(LiquidPlacementModule copyFrom = null)
	{
		if (copyFrom == null)
		{
			water = LiquidPlacement.Allowed;
			lava = LiquidPlacement.Allowed;
		}
		else
		{
			water = copyFrom.water;
			lava = copyFrom.lava;
		}
	}
}
