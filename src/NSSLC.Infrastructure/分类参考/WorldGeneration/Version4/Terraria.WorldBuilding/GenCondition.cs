namespace Terraria.WorldBuilding;

public abstract class GenCondition : GenBase
{
	protected abstract bool CheckValidity(int x, int y);
}
