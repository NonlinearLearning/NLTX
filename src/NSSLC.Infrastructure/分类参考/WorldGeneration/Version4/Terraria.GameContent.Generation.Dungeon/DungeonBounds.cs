using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using ReLogic.Utilities;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon;

public class DungeonBounds
{
	[JsonProperty]
	private Rectangle? _hitbox;

	private int _boundsLeft;

	private int _boundsRight;

	private int _boundsTop;

	private int _boundsBottom;

	public int X => _boundsLeft;

	public int Y => _boundsTop;

	public int Width => _boundsRight - _boundsLeft;

	public int Height => _boundsBottom - _boundsTop;

	public int Left
	{
		get
		{
			return _boundsLeft;
		}
		set
		{
			_boundsLeft = (int)MathHelper.Clamp(value, 10f, Main.maxTilesX - 10);
		}
	}

	public int Right
	{
		get
		{
			return _boundsRight;
		}
		set
		{
			_boundsRight = (int)MathHelper.Clamp(value, 10f, Main.maxTilesX - 10);
		}
	}

	public int Top
	{
		get
		{
			return _boundsTop;
		}
		set
		{
			_boundsTop = (int)MathHelper.Clamp(value, 10f, Main.maxTilesY - 10);
		}
	}

	public int Bottom
	{
		get
		{
			return _boundsBottom;
		}
		set
		{
			_boundsBottom = (int)MathHelper.Clamp(value, 10f, Main.maxTilesY - 10);
		}
	}

	public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);
	public void Inflate(int amount)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonBounds.Inflate"))
	{
		SetBounds(Left - amount, Top - amount, Right + amount, Bottom + amount);
	}
	}
	public bool ContainsWithFluff(int x, int y, int fluff)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonBounds.ContainsWithFluff"))
	{
		if (fluff == 0)
		{
			return Contains(x, y);
		}
		if (!_hitbox.HasValue)
		{
			return false;
		}
		Rectangle rectangle = new Rectangle(_hitbox.Value.Left - fluff, _hitbox.Value.Top - fluff, _hitbox.Value.Width + fluff * 2, _hitbox.Value.Height + fluff * 2);
		return rectangle.Contains(x, y);
	}
	}
	public bool Contains(int x, int y)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonBounds.Contains"))
	{
		if (!_hitbox.HasValue)
		{
			return false;
		}
		return _hitbox.Value.Contains(x, y);
	}
	}
	public bool Intersects(Rectangle hitbox)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonBounds.Intersects"))
	{
		if (!_hitbox.HasValue)
		{
			return false;
		}
		return _hitbox.Value.Intersects(hitbox);
	}
	}
	public void SetBounds(int minX, int minY, int maxX, int maxY)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonBounds.SetBounds"))
	{
		Left = minX;
		Right = maxX;
		Top = minY;
		Bottom = maxY;
		CalculateHitbox();
	}
	}
	public Rectangle CalculateHitbox()
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonBounds.CalculateHitbox"))
	{
		if (Right <= Left)
		{
			Right = Left + 1;
		}
		if (Bottom <= Top)
		{
			Bottom = Top + 1;
		}
		_hitbox = new Rectangle(X, Y, Width, Height);
		return _hitbox.Value;
	}
	}
	public void Reset()
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonBounds.Reset"))
	{
		_hitbox = null;
		Left = 0;
		Right = 0;
		Top = 0;
		Bottom = 0;
	}
	}}
