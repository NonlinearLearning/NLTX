using Microsoft.Xna.Framework;

namespace Terraria.WorldBuilding;

public static class Searches
{
	public class Left : GenSearch
	{
		private int _maxDistance;

		public Left(int maxDistance)
		{
			_maxDistance = maxDistance;
		}

		public override Point Find(Point origin){
  return new Point();
}	}

	public class Right : GenSearch
	{
		private int _maxDistance;

		public Right(int maxDistance)
		{
			_maxDistance = maxDistance;
		}

		public override Point Find(Point origin){
  return new Point();
}	}

	public class Down : GenSearch
	{
		private int _maxDistance;

		public Down(int maxDistance)
		{
			_maxDistance = maxDistance;
		}

		public override Point Find(Point origin){
  return new Point();
}	}

	public class Up : GenSearch
	{
		private int _maxDistance;

		public Up(int maxDistance)
		{
			_maxDistance = maxDistance;
		}

		public override Point Find(Point origin){
  return new Point();
}	}

	public class Rectangle : GenSearch
	{
		private int _width;

		private int _height;

		public Rectangle(int width, int height)
		{
			_width = width;
			_height = height;
		}

		public override Point Find(Point origin){
  return new Point();
}	}

	public static GenSearch Chain(GenSearch search, params GenCondition[] conditions)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.Searches.Chain"))
	{
		return search.Conditions(conditions);
	}
	}}
