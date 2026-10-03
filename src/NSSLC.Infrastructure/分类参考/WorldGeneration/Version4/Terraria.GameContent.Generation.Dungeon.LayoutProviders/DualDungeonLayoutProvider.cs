using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Biomes;
using Terraria.GameContent.Generation.Dungeon.Halls;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.LayoutProviders;

public class DualDungeonLayoutProvider : DungeonLayoutProvider
{
	private class HallwayCalculator
	{
		private class RoomEntry
		{
			public DungeonRoom room;

			public double progressAlongSnake;

			public List<RoomEntry> backLinks = new List<RoomEntry>();

			public List<RoomEntry> forwardLinks = new List<RoomEntry>();
		}

		private class HallLine
		{
			public RoomEntry source;

			public RoomEntry target;

			public Vector2D sourcePoint;

			public Vector2D targetPoint;
		}

		private readonly DungeonData data;

		private readonly List<RoomEntry> rooms;

		private readonly List<DungeonHall> halls = new List<DungeonHall>();

		private readonly List<HallLine> stairwells = new List<HallLine>();

		private readonly DitherSnake controlLines;

		private readonly double maxProgressDelta;

		private readonly double avgLineLength;

		public HallwayCalculator(DungeonData data, List<DungeonRoom> rooms)
		{
			this.data = data;
			this.rooms = (from r in rooms
				select new RoomEntry
				{
					room = r,
					progressAlongSnake = data.genVars.dungeonDitherSnake.GetPositionAlongSnake((Vector2D)r.Center)
				} into r
				orderby r.progressAlongSnake
				select r).ToList();
			controlLines = data.genVars.dungeonDitherSnake;
			avgLineLength = controlLines.Average((DungeonControlLine l) => l.LineLength);
			maxProgressDelta = 300.0 / avgLineLength;
		}
}

	public DualDungeonLayoutProvider(DungeonLayoutProviderSettings settings)
		: base(settings)
	{
	}

	public override void ProvideLayout(DungeonData data, GenerationProgress progress, UnifiedRandom
  genRand, ref int roomDelay){}
}
