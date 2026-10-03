using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class RegularDungeonRoom : DungeonRoom
{
	public int _innerBoundsSize;

	public RegularDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
	}

	public override void CalculateRoom(DungeonData data){}
	public override bool GenerateRoom(DungeonData data){
  return new bool ();
}
}
