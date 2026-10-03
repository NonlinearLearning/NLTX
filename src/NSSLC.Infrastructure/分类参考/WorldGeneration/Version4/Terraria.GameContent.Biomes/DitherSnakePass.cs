using System;
using System.Collections.Generic;
using System.Linq;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes;

public class DitherSnakePass : GenPass
{

	public DitherSnakePass(string passName)
		: base(passName, 1.0)
	{
	}

	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration){}
}
