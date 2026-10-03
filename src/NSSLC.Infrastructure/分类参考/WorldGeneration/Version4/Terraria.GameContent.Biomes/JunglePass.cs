using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Biomes;

public class JunglePass : GenPass
{

	public JunglePass()
		: base(GenPassNameID.Jungle, 10154.65234375)
	{
	}

	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration){}
}
