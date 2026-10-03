namespace Terraria.WorldBuilding;

public static class Conditions
{
	public class IsTile : GenCondition
	{
		private ushort[] _types;

		public IsTile(params ushort[] types)
		{
			_types = types;
		}

		protected override bool CheckValidity(int x, int y){
  return new bool ();
}	}

	public class Continue : GenCondition
	{
		protected override bool CheckValidity(int x, int y){
  return new bool ();
}	}

	public class BoolCheck : GenCondition
	{
		private bool _theBool;

		public BoolCheck(bool theBool)
		{
			_theBool = theBool;
		}

		protected override bool CheckValidity(int x, int y){
  return new bool ();
}	}

	public class MysticSnake : GenCondition
	{
		protected override bool CheckValidity(int x, int y){
  return new bool ();
}	}

	public class InWorld : GenCondition
	{
		private int _fluff;

		public InWorld(int fluff)
		{
			_fluff = fluff;
		}

		protected override bool CheckValidity(int x, int y){
  return new bool ();
}	}

	public class IsSolid : GenCondition
	{
		protected override bool CheckValidity(int x, int y){
  return new bool ();
}	}

	public class HasLava : GenCondition
	{
		protected override bool CheckValidity(int x, int y){
  return new bool ();
}	}

	public class NotNull : GenCondition
	{
		protected override bool CheckValidity(int x, int y){
  return new bool ();
}	}
}
