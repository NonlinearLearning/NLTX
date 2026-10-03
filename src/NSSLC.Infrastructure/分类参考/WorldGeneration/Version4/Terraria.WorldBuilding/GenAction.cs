using Microsoft.Xna.Framework;

namespace Terraria.WorldBuilding;

public abstract class GenAction : GenBase
{
	public GenAction NextAction;

	public ShapeData OutputData;

	public abstract bool Apply(Point origin, int x, int y, params object[] args);
	public GenAction Output(ShapeData data)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.GenAction.Output"))
	{
		OutputData = data;
		return this;
	}
	}}
