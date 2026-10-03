using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria.ID;

namespace Terraria.WorldBuilding;

public class StructureMap
{
	[JsonProperty]
	private readonly List<Rectangle> _structures = new List<Rectangle>(2048);

	[JsonProperty]
	private readonly List<Rectangle> _protectedStructures = new List<Rectangle>(2048);

	private readonly object _lock = new object();

	public bool CanPlace(Rectangle area, int padding = 0)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.StructureMap.CanPlace"))
	{
		return CanPlace(area, TileID.Sets.GeneralPlacementTiles, padding);
	}
	}
	public bool CanPlace(Rectangle area, bool[] validTiles, int padding = 0)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.StructureMap.CanPlace"))
	{
		lock (_lock)
		{
			if (area.X < 0 || area.Y < 0 || area.X + area.Width > Main.maxTilesX - 1 || area.Y + area.Height > Main.maxTilesY - 1)
			{
				return false;
			}
			Rectangle rectangle = new Rectangle(area.X - padding, area.Y - padding, area.Width + padding * 2, area.Height + padding * 2);
			for (int i = 0; i < _protectedStructures.Count; i++)
			{
				if (rectangle.Intersects(_protectedStructures[i]))
				{
					return false;
				}
			}
			for (int j = rectangle.X; j < rectangle.X + rectangle.Width; j++)
			{
				for (int k = rectangle.Y; k < rectangle.Y + rectangle.Height; k++)
				{
					if (Main.tile[j, k].active())
					{
						ushort type = Main.tile[j, k].type;
						if (!validTiles[type])
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}
	}
	public void AddProtectedStructure(Rectangle area, int padding = 0)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.StructureMap.AddProtectedStructure"))
	{
		lock (_lock)
		{
			area.Inflate(padding, padding);
			_structures.Add(area);
			_protectedStructures.Add(area);
		}
	}
	}
}
