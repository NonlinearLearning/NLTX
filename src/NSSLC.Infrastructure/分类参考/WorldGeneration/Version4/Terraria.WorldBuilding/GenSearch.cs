using Microsoft.Xna.Framework;

namespace Terraria.WorldBuilding;

public abstract class GenSearch : GenBase
{
	public static Point NOT_FOUND = new Point(int.MaxValue, int.MaxValue);

	private GenCondition[] _conditions;

	public GenSearch Conditions(params GenCondition[] conditions)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.GenSearch.Conditions"))
	{
		_conditions = conditions;
		return this;
	}
	}
	public abstract Point Find(Point origin);
}
