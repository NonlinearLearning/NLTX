using Microsoft.Xna.Framework;

namespace Terraria.WorldBuilding;

public abstract class GenShape : GenBase
{

	protected bool _quitOnFail;

	public abstract bool Perform(Point origin, GenAction action);
}
