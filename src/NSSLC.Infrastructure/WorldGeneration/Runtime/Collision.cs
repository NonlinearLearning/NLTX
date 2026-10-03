using System;
using System.Collections.Generic;
using NSSLC.WorldGeneration.Geometry;
using NSSLC.WorldGeneration.DataStructures;
using NSSLC.WorldGeneration.ID;

namespace NSSLC.WorldGeneration;

public class Collision
{
	public enum TileContactSide
	{
		Left,
		Right,
		Top,
		Bottom,
		BottomLeft,
		BottomRight
	}

	public struct TileContact
	{
		public TileContactSide Side;

		public int Overlap;

		public int X;

		public int Y;

		public int Slope;

		public int Type;

		public TileContact(TileContactSide side, int x, int y, int type, int slope, int overlap)
		{
			Side = side;
			X = x;
			Y = y;
			Slope = slope;
			Type = type;
			Overlap = overlap;
		}
	}

	public struct HurtTile
	{
		public int type;

		public int x;

		public int y;
	}

	public static bool stair;

	public static bool stairFall;

	public static bool honey;

	public static bool shimmer;

	public static bool sloping;

	public static bool landMine = false;

	public static bool up;

	public static bool down;

	public static bool CanHit(Vector2 Position1, int Width1, int Height1, Vector2 Position2, int Width2, int Height2)
{
	
		return CanHit(Position1.ToPoint(), Width1, Height1, Position2.ToPoint(), Width2, Height2);
	
	}
	public static bool CanHit(Point Position1, int Width1, int Height1, Point Position2, int Width2, int Height2)
{
	
		int num = (Position1.X + Width1 / 2) / 16;
		int num2 = (Position1.Y + Height1 / 2) / 16;
		int num3 = (Position2.X + Width2 / 2) / 16;
		int num4 = (Position2.Y + Height2 / 2) / 16;
		if (num <= 1)
		{
			num = 1;
		}
		if (num >= Main.maxTilesX)
		{
			num = Main.maxTilesX - 1;
		}
		if (num3 <= 1)
		{
			num3 = 1;
		}
		if (num3 >= Main.maxTilesX)
		{
			num3 = Main.maxTilesX - 1;
		}
		if (num2 <= 1)
		{
			num2 = 1;
		}
		if (num2 >= Main.maxTilesY - 40)
		{
			num2 = Main.maxTilesY - 40;
		}
		if (num4 <= 1)
		{
			num4 = 1;
		}
		if (num4 >= Main.maxTilesY - 40)
		{
			num4 = Main.maxTilesY - 40;
		}
		try
		{
			do
			{
				int num5 = Math.Abs(num - num3);
				int num6 = Math.Abs(num2 - num4);
				if (num == num3 && num2 == num4)
				{
					return true;
				}
				if (num5 > num6)
				{
					num = ((num >= num3) ? (num - 1) : (num + 1));
					if (Main.tile[num, num2 - 1] == null)
					{
						return false;
					}
					if (Main.tile[num, num2 + 1] == null)
					{
						return false;
					}
					if (!Main.tile[num, num2 - 1].inActive() && Main.tile[num, num2 - 1].active() && Main.tileSolid[Main.tile[num, num2 - 1].type] && !Main.tileSolidTop[Main.tile[num, num2 - 1].type] && Main.tile[num, num2 - 1].slope() == 0 && !Main.tile[num, num2 - 1].halfBrick() && !Main.tile[num, num2 + 1].inActive() && Main.tile[num, num2 + 1].active() && Main.tileSolid[Main.tile[num, num2 + 1].type] && !Main.tileSolidTop[Main.tile[num, num2 + 1].type] && Main.tile[num, num2 + 1].slope() == 0 && !Main.tile[num, num2 + 1].halfBrick())
					{
						return false;
					}
				}
				else
				{
					num2 = ((num2 >= num4) ? (num2 - 1) : (num2 + 1));
					if (Main.tile[num - 1, num2] == null)
					{
						return false;
					}
					if (Main.tile[num + 1, num2] == null)
					{
						return false;
					}
					if (!Main.tile[num - 1, num2].inActive() && Main.tile[num - 1, num2].active() && Main.tileSolid[Main.tile[num - 1, num2].type] && !Main.tileSolidTop[Main.tile[num - 1, num2].type] && Main.tile[num - 1, num2].slope() == 0 && !Main.tile[num - 1, num2].halfBrick() && !Main.tile[num + 1, num2].inActive() && Main.tile[num + 1, num2].active() && Main.tileSolid[Main.tile[num + 1, num2].type] && !Main.tileSolidTop[Main.tile[num + 1, num2].type] && Main.tile[num + 1, num2].slope() == 0 && !Main.tile[num + 1, num2].halfBrick())
					{
						return false;
					}
				}
				if (Main.tile[num, num2] == null)
				{
					return false;
				}
			}
			while (Main.tile[num, num2].inActive() || !Main.tile[num, num2].active() || !Main.tileSolid[Main.tile[num, num2].type] || Main.tileSolidTop[Main.tile[num, num2].type]);
			return false;
		}
		catch
		{
			return false;
		}
	
	}
	public static bool CanHitWithCheck(Vector2 Position1, int Width1, int Height1, Vector2 Position2, int Width2, int Height2, Utils.TileActionAttempt check)
{
	
		int num = (int)((Position1.X + (float)(Width1 / 2)) / 16f);
		int num2 = (int)((Position1.Y + (float)(Height1 / 2)) / 16f);
		int num3 = (int)((Position2.X + (float)(Width2 / 2)) / 16f);
		int num4 = (int)((Position2.Y + (float)(Height2 / 2)) / 16f);
		if (num <= 1)
		{
			num = 1;
		}
		if (num >= Main.maxTilesX)
		{
			num = Main.maxTilesX - 1;
		}
		if (num3 <= 1)
		{
			num3 = 1;
		}
		if (num3 >= Main.maxTilesX)
		{
			num3 = Main.maxTilesX - 1;
		}
		if (num2 <= 1)
		{
			num2 = 1;
		}
		if (num2 >= Main.maxTilesY - 40)
		{
			num2 = Main.maxTilesY - 40;
		}
		if (num4 <= 1)
		{
			num4 = 1;
		}
		if (num4 >= Main.maxTilesY - 40)
		{
			num4 = Main.maxTilesY - 40;
		}
		try
		{
			do
			{
				int num5 = Math.Abs(num - num3);
				int num6 = Math.Abs(num2 - num4);
				if (num == num3 && num2 == num4)
				{
					return true;
				}
				if (num5 > num6)
				{
					num = ((num >= num3) ? (num - 1) : (num + 1));
					if (Main.tile[num, num2 - 1] == null)
					{
						return false;
					}
					if (Main.tile[num, num2 + 1] == null)
					{
						return false;
					}
					if (!Main.tile[num, num2 - 1].inActive() && Main.tile[num, num2 - 1].active() && Main.tileSolid[Main.tile[num, num2 - 1].type] && !Main.tileSolidTop[Main.tile[num, num2 - 1].type] && Main.tile[num, num2 - 1].slope() == 0 && !Main.tile[num, num2 - 1].halfBrick() && !Main.tile[num, num2 + 1].inActive() && Main.tile[num, num2 + 1].active() && Main.tileSolid[Main.tile[num, num2 + 1].type] && !Main.tileSolidTop[Main.tile[num, num2 + 1].type] && Main.tile[num, num2 + 1].slope() == 0 && !Main.tile[num, num2 + 1].halfBrick())
					{
						return false;
					}
				}
				else
				{
					num2 = ((num2 >= num4) ? (num2 - 1) : (num2 + 1));
					if (Main.tile[num - 1, num2] == null)
					{
						return false;
					}
					if (Main.tile[num + 1, num2] == null)
					{
						return false;
					}
					if (!Main.tile[num - 1, num2].inActive() && Main.tile[num - 1, num2].active() && Main.tileSolid[Main.tile[num - 1, num2].type] && !Main.tileSolidTop[Main.tile[num - 1, num2].type] && Main.tile[num - 1, num2].slope() == 0 && !Main.tile[num - 1, num2].halfBrick() && !Main.tile[num + 1, num2].inActive() && Main.tile[num + 1, num2].active() && Main.tileSolid[Main.tile[num + 1, num2].type] && !Main.tileSolidTop[Main.tile[num + 1, num2].type] && Main.tile[num + 1, num2].slope() == 0 && !Main.tile[num + 1, num2].halfBrick())
					{
						return false;
					}
				}
				if (Main.tile[num, num2] == null)
				{
					return false;
				}
				if (!Main.tile[num, num2].inActive() && Main.tile[num, num2].active() && Main.tileSolid[Main.tile[num, num2].type] && !Main.tileSolidTop[Main.tile[num, num2].type])
				{
					return false;
				}
			}
			while (check(num, num2));
			return false;
		}
		catch
		{
			return false;
		}
	
	}
	public static bool CanHitLine(Vector2 Position1, int Width1, int Height1, Vector2 Position2, int Width2, int Height2)
{
	
		int num = (int)((Position1.X + (float)(Width1 / 2)) / 16f);
		int num2 = (int)((Position1.Y + (float)(Height1 / 2)) / 16f);
		int num3 = (int)((Position2.X + (float)(Width2 / 2)) / 16f);
		int num4 = (int)((Position2.Y + (float)(Height2 / 2)) / 16f);
		if (num <= 1)
		{
			num = 1;
		}
		if (num >= Main.maxTilesX)
		{
			num = Main.maxTilesX - 1;
		}
		if (num3 <= 1)
		{
			num3 = 1;
		}
		if (num3 >= Main.maxTilesX)
		{
			num3 = Main.maxTilesX - 1;
		}
		if (num2 <= 1)
		{
			num2 = 1;
		}
		if (num2 >= Main.maxTilesY - 40)
		{
			num2 = Main.maxTilesY - 40;
		}
		if (num4 <= 1)
		{
			num4 = 1;
		}
		if (num4 >= Main.maxTilesY - 40)
		{
			num4 = Main.maxTilesY - 40;
		}
		float num5 = Math.Abs(num - num3);
		float num6 = Math.Abs(num2 - num4);
		if (num5 == 0f && num6 == 0f)
		{
			return true;
		}
		float num7 = 1f;
		float num8 = 1f;
		if (num5 == 0f || num6 == 0f)
		{
			if (num5 == 0f)
			{
				num7 = 0f;
			}
			if (num6 == 0f)
			{
				num8 = 0f;
			}
		}
		else if (num5 > num6)
		{
			num7 = num5 / num6;
		}
		else
		{
			num8 = num6 / num5;
		}
		float num9 = 0f;
		float num10 = 0f;
		int num11 = 1;
		if (num2 < num4)
		{
			num11 = 2;
		}
		int num12 = (int)num5;
		int num13 = (int)num6;
		int num14 = Math.Sign(num3 - num);
		int num15 = Math.Sign(num4 - num2);
		bool flag = false;
		bool flag2 = false;
		try
		{
			do
			{
				switch (num11)
				{
				case 2:
				{
					num9 += num7;
					int num17 = (int)num9;
					num9 -= (float)num17;
					for (int j = 0; j < num17; j++)
					{
						if (Main.tile[num, num2 - 1] == null)
						{
							return false;
						}
						if (Main.tile[num, num2] == null)
						{
							return false;
						}
						if (Main.tile[num, num2 + 1] == null)
						{
							return false;
						}
						Tile tile4 = Main.tile[num, num2 - 1];
						Tile tile5 = Main.tile[num, num2 + 1];
						Tile tile6 = Main.tile[num, num2];
						if ((!tile4.inActive() && tile4.active() && Main.tileSolid[tile4.type] && !Main.tileSolidTop[tile4.type]) || (!tile5.inActive() && tile5.active() && Main.tileSolid[tile5.type] && !Main.tileSolidTop[tile5.type]) || (!tile6.inActive() && tile6.active() && Main.tileSolid[tile6.type] && !Main.tileSolidTop[tile6.type]))
						{
							return false;
						}
						if (num12 == 0 && num13 == 0)
						{
							flag = true;
							break;
						}
						num += num14;
						num12--;
						if (num12 == 0 && num13 == 0 && num17 == 1)
						{
							flag2 = true;
						}
					}
					if (num13 != 0)
					{
						num11 = 1;
					}
					break;
				}
				case 1:
				{
					num10 += num8;
					int num16 = (int)num10;
					num10 -= (float)num16;
					for (int i = 0; i < num16; i++)
					{
						if (Main.tile[num - 1, num2] == null)
						{
							return false;
						}
						if (Main.tile[num, num2] == null)
						{
							return false;
						}
						if (Main.tile[num + 1, num2] == null)
						{
							return false;
						}
						Tile tile = Main.tile[num - 1, num2];
						Tile tile2 = Main.tile[num + 1, num2];
						Tile tile3 = Main.tile[num, num2];
						if ((!tile.inActive() && tile.active() && Main.tileSolid[tile.type] && !Main.tileSolidTop[tile.type]) || (!tile2.inActive() && tile2.active() && Main.tileSolid[tile2.type] && !Main.tileSolidTop[tile2.type]) || (!tile3.inActive() && tile3.active() && Main.tileSolid[tile3.type] && !Main.tileSolidTop[tile3.type]))
						{
							return false;
						}
						if (num12 == 0 && num13 == 0)
						{
							flag = true;
							break;
						}
						num2 += num15;
						num13--;
						if (num12 == 0 && num13 == 0 && num16 == 1)
						{
							flag2 = true;
						}
					}
					if (num12 != 0)
					{
						num11 = 2;
					}
					break;
				}
				}
				if (Main.tile[num, num2] == null)
				{
					return false;
				}
				Tile tile7 = Main.tile[num, num2];
				if (!tile7.inActive() && tile7.active() && Main.tileSolid[tile7.type] && !Main.tileSolidTop[tile7.type])
				{
					return false;
				}
			}
			while (!(flag || flag2));
			return true;
		}
		catch
		{
			return false;
		}
	
	}
	public static bool EmptyTile(int i, int j, bool ignoreTiles = false)
{
	
		Rectangle rectangle = new Rectangle(i * 16, j * 16, 16, 16);
		if (Main.tile[i, j].active() && !ignoreTiles)
		{
			return false;
		}
		for (int k = 0; k < 255; k++)
		{
			if (Main.player[k].active && !Main.player[k].dead && !Main.player[k].ghost && rectangle.Intersects(new Rectangle((int)Main.player[k].position.X, (int)Main.player[k].position.Y, Main.player[k].width, Main.player[k].height)))
			{
				return false;
			}
		}
		for (int l = 0; l < Main.maxNPCs; l++)
		{
			if (Main.npc[l].active && rectangle.Intersects(new Rectangle((int)Main.npc[l].position.X, (int)Main.npc[l].position.Y, Main.npc[l].width, Main.npc[l].height)))
			{
				return false;
			}
		}
		return true;
	
	}
	public static bool WetCollision(Vector2 Position, int Width, int Height)
{
	
		honey = false;
		shimmer = false;
		Vector2 vector = new Vector2(Position.X + (float)(Width / 2), Position.Y + (float)(Height / 2));
		int num = 10;
		int num2 = Height / 2;
		if (num > Width)
		{
			num = Width;
		}
		if (num2 > Height)
		{
			num2 = Height;
		}
		vector = new Vector2(vector.X - (float)(num / 2), vector.Y - (float)(num2 / 2));
		int value = (int)(Position.X / 16f) - 1;
		int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
		int value3 = (int)(Position.Y / 16f) - 1;
		int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
		int num3 = Utils.Clamp(value, 0, Main.maxTilesX - 1);
		value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
		value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 40);
		value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 40);
		Vector2 vector2 = default(Vector2);
		for (int i = num3; i < value2; i++)
		{
			for (int j = value3; j < value4; j++)
			{
				if (Main.tile[i, j] == null)
				{
					continue;
				}
				if (Main.tile[i, j].liquid > 0)
				{
					vector2.X = i * 16;
					vector2.Y = j * 16;
					int num4 = 16;
					float num5 = 256 - Main.tile[i, j].liquid;
					num5 /= 32f;
					vector2.Y += num5 * 2f;
					num4 -= (int)(num5 * 2f);
					if (vector.X + (float)num > vector2.X && vector.X < vector2.X + 16f && vector.Y + (float)num2 > vector2.Y && vector.Y < vector2.Y + (float)num4)
					{
						if (Main.tile[i, j].honey())
						{
							honey = true;
						}
						if (Main.tile[i, j].shimmer())
						{
							shimmer = true;
						}
						return true;
					}
				}
				else
				{
					if (!Main.tile[i, j].active() || Main.tile[i, j].slope() == 0 || j <= 0 || Main.tile[i, j - 1] == null || Main.tile[i, j - 1].liquid <= 0)
					{
						continue;
					}
					vector2.X = i * 16;
					vector2.Y = j * 16;
					int num6 = 16;
					if (vector.X + (float)num > vector2.X && vector.X < vector2.X + 16f && vector.Y + (float)num2 > vector2.Y && vector.Y < vector2.Y + (float)num6)
					{
						if (Main.tile[i, j - 1].honey())
						{
							honey = true;
						}
						else if (Main.tile[i, j - 1].shimmer())
						{
							shimmer = true;
						}
						return true;
					}
				}
			}
		}
		return false;
	
	}
	public static bool LavaCollision(Vector2 Position, int Width, int Height)
{
	
		int value = (int)(Position.X / 16f) - 1;
		int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
		int value3 = (int)(Position.Y / 16f) - 1;
		int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
		int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
		value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
		value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 40);
		value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 40);
		Vector2 vector = default(Vector2);
		for (int i = num; i < value2; i++)
		{
			for (int j = value3; j < value4; j++)
			{
				if (Main.tile[i, j] != null && Main.tile[i, j].liquid > 0 && Main.tile[i, j].lava())
				{
					vector.X = i * 16;
					vector.Y = j * 16;
					int num2 = 16;
					float num3 = 256 - Main.tile[i, j].liquid;
					num3 /= 32f;
					vector.Y += num3 * 2f;
					num2 -= (int)(num3 * 2f);
					if (Position.X + (float)Width > vector.X && Position.X < vector.X + 16f && Position.Y + (float)Height > vector.Y && Position.Y < vector.Y + (float)num2)
					{
						return true;
					}
				}
			}
		}
		return false;
	
	}
	public static Vector4 SlopeCollision(Vector2 Position, Vector2 Velocity, int Width, int Height, float gravity = 0f, bool fall = false, bool ignoreAetheriumPlatforms = false)
{
	
		stair = false;
		stairFall = false;
		BitsByte bitsByte = (byte)0;
		float y = Position.Y;
		float y2 = Position.Y;
		sloping = false;
		Vector2 vector = Position;
		Vector2 vector2 = Position;
		Vector2 vector3 = Velocity;
		int value = (int)(Position.X / 16f) - 1;
		int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
		int value3 = (int)(Position.Y / 16f) - 1;
		int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
		int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
		value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
		value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 40);
		value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 40);
		Vector2 vector4 = default(Vector2);
		for (int i = num; i < value2; i++)
		{
			for (int j = value3; j < value4; j++)
			{
				Tile tile = Main.tile[i, j];
				if (tile == null || !tile.active() || tile.inActive())
				{
					continue;
				}
				bool flag = Main.tileSolid[tile.type];
				if (Main.tileSolidTop[tile.type] && tile.frameY == 0)
				{
					flag = true;
				}
				if (ignoreAetheriumPlatforms && tile.type == 19 && tile.frameY / 18 == 50)
				{
					flag = false;
				}
				if (!flag)
				{
					continue;
				}
				vector4.X = i * 16;
				vector4.Y = j * 16;
				int num2 = 16;
				if (Main.tile[i, j].halfBrick())
				{
					vector4.Y += 8f;
					num2 -= 8;
				}
				if (!(Position.X + (float)Width > vector4.X) || !(Position.X < vector4.X + 16f) || !(Position.Y + (float)Height > vector4.Y) || !(Position.Y < vector4.Y + (float)num2))
				{
					continue;
				}
				bool flag2 = true;
				if (TileID.Sets.Platforms[Main.tile[i, j].type])
				{
					if (Velocity.Y < 0f)
					{
						flag2 = false;
					}
					if (Position.Y + (float)Height < (float)(j * 16) || Position.Y + (float)Height - (1f + Math.Abs(Velocity.X)) > (float)(j * 16 + 16))
					{
						flag2 = false;
					}
					if (((Main.tile[i, j].slope() == 1 && Velocity.X >= 0f) || (Main.tile[i, j].slope() == 2 && Velocity.X <= 0f)) && (Position.Y + (float)Height) / 16f - 1f == (float)j)
					{
						flag2 = false;
					}
				}
				if (!flag2)
				{
					continue;
				}
				bool flag3 = false;
				if (fall && TileID.Sets.Platforms[Main.tile[i, j].type])
				{
					flag3 = true;
				}
				int num3 = Main.tile[i, j].slope();
				vector4.X = i * 16;
				vector4.Y = j * 16;
				if (!(Position.X + (float)Width > vector4.X) || !(Position.X < vector4.X + 16f) || !(Position.Y + (float)Height > vector4.Y) || !(Position.Y < vector4.Y + 16f))
				{
					continue;
				}
				float num4 = 0f;
				if (num3 == 3 || num3 == 4)
				{
					if (num3 == 3)
					{
						num4 = Position.X - vector4.X;
					}
					if (num3 == 4)
					{
						num4 = vector4.X + 16f - (Position.X + (float)Width);
					}
					if (num4 >= 0f)
					{
						if (Position.Y <= vector4.Y + 16f - num4)
						{
							float num5 = vector4.Y + 16f - vector.Y - num4;
							if (Position.Y + num5 > y2)
							{
								vector2.Y = Position.Y + num5;
								y2 = vector2.Y;
								if (vector3.Y < 0.0101f)
								{
									vector3.Y = 0.0101f;
								}
								bitsByte[num3] = true;
							}
						}
					}
					else if (Position.Y > vector4.Y)
					{
						float num6 = vector4.Y + 16f;
						if (vector2.Y < num6)
						{
							vector2.Y = num6;
							if (vector3.Y < 0.0101f)
							{
								vector3.Y = 0.0101f;
							}
						}
					}
				}
				if (num3 != 1 && num3 != 2)
				{
					continue;
				}
				if (num3 == 1)
				{
					num4 = Position.X - vector4.X;
				}
				if (num3 == 2)
				{
					num4 = vector4.X + 16f - (Position.X + (float)Width);
				}
				if (num4 >= 0f)
				{
					if (!(Position.Y + (float)Height >= vector4.Y + num4))
					{
						continue;
					}
					float num7 = vector4.Y - (vector.Y + (float)Height) + num4;
					if (!(Position.Y + num7 < y))
					{
						continue;
					}
					if (flag3)
					{
						stairFall = true;
						continue;
					}
					if (TileID.Sets.Platforms[Main.tile[i, j].type])
					{
						stair = true;
					}
					else
					{
						stair = false;
					}
					vector2.Y = Position.Y + num7;
					y = vector2.Y;
					if (vector3.Y > 0f)
					{
						vector3.Y = 0f;
					}
					bitsByte[num3] = true;
					continue;
				}
				if (TileID.Sets.Platforms[Main.tile[i, j].type] && !(Position.Y + (float)Height - 4f - Math.Abs(Velocity.X) <= vector4.Y))
				{
					if (flag3)
					{
						stairFall = true;
					}
					continue;
				}
				float num8 = vector4.Y - (float)Height;
				if (!(vector2.Y > num8))
				{
					continue;
				}
				if (flag3)
				{
					stairFall = true;
					continue;
				}
				if (TileID.Sets.Platforms[Main.tile[i, j].type])
				{
					stair = true;
				}
				else
				{
					stair = false;
				}
				vector2.Y = num8;
				if (vector3.Y > 0f)
				{
					vector3.Y = 0f;
				}
			}
		}
		Vector2 position = Position;
		Vector2 velocity = vector2 - Position;
		Vector2 vector5 = TileCollision(position, velocity, Width, Height);
		if (vector5.Y > velocity.Y)
		{
			float num9 = velocity.Y - vector5.Y;
			vector2.Y = Position.Y + vector5.Y;
			if (bitsByte[1])
			{
				vector2.X = Position.X - num9;
			}
			if (bitsByte[2])
			{
				vector2.X = Position.X + num9;
			}
			vector3.X = 0f;
			vector3.Y = 0f;
			up = false;
		}
		else if (vector5.Y < velocity.Y)
		{
			float num10 = vector5.Y - velocity.Y;
			vector2.Y = Position.Y + vector5.Y;
			if (bitsByte[3])
			{
				vector2.X = Position.X - num10;
			}
			if (bitsByte[4])
			{
				vector2.X = Position.X + num10;
			}
			vector3.X = 0f;
			vector3.Y = 0f;
		}
		return new Vector4(vector2, vector3.X, vector3.Y);
	
	}
	public static Vector2 TileCollision(Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough = false, bool fall2 = false, int gravDir = 1, bool ignoreDoors = false, bool ignoreAetheriumPlatforms = false, bool hoik = true)
{
	
		up = false;
		down = false;
		Vector2 result = Velocity;
		Vector2 vector = Velocity;
		Vector2 vector2 = Position + Velocity;
		Vector2 vector3 = Position;
		int value = (int)(Position.X / 16f) - 1;
		int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
		int value3 = (int)(Position.Y / 16f) - 1;
		int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		int num5 = Utils.Clamp(value, 0, Main.maxTilesX - 1);
		value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
		value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 40);
		value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 40);
		float num6 = (value4 + 3) * 16;
		Vector2 vector4 = default(Vector2);
		for (int i = num5; i < value2; i++)
		{
			for (int j = value3; j < value4; j++)
			{
				Tile tile = Main.tile[i, j];
				if (tile == null || !tile.active() || tile.inActive())
				{
					continue;
				}
				bool flag = Main.tileSolid[tile.type];
				if (Main.tileSolidTop[tile.type] && tile.frameY == 0)
				{
					flag = true;
				}
				if (ignoreDoors && TileID.Sets.ForAdvancedCollision.ClosedDoors[tile.type])
				{
					flag = false;
				}
				if (ignoreAetheriumPlatforms && tile.type == 19 && tile.frameY / 18 == 50)
				{
					flag = false;
				}
				if (!flag)
				{
					continue;
				}
				vector4.X = i * 16;
				vector4.Y = j * 16;
				int num7 = 16;
				if (Main.tile[i, j].halfBrick())
				{
					vector4.Y += 8f;
					num7 -= 8;
				}
				if (!(vector2.X + (float)Width > vector4.X) || !(vector2.X < vector4.X + 16f) || !(vector2.Y + (float)Height > vector4.Y) || !(vector2.Y < vector4.Y + (float)num7))
				{
					continue;
				}
				bool flag2 = false;
				bool flag3 = false;
				if (Main.tile[i, j].slope() > 2)
				{
					if (Main.tile[i, j].slope() == 3 && vector3.Y + Math.Abs(Velocity.X) >= vector4.Y && vector3.X >= vector4.X)
					{
						flag3 = true;
					}
					if (Main.tile[i, j].slope() == 4 && vector3.Y + Math.Abs(Velocity.X) >= vector4.Y && vector3.X + (float)Width <= vector4.X + 16f)
					{
						flag3 = true;
					}
				}
				else if (Main.tile[i, j].slope() > 0)
				{
					flag2 = true;
					if (Main.tile[i, j].slope() == 1 && vector3.Y + (float)Height - Math.Abs(Velocity.X) <= vector4.Y + (float)num7 && vector3.X >= vector4.X)
					{
						flag3 = true;
					}
					if (Main.tile[i, j].slope() == 2 && vector3.Y + (float)Height - Math.Abs(Velocity.X) <= vector4.Y + (float)num7 && vector3.X + (float)Width <= vector4.X + 16f)
					{
						flag3 = true;
					}
				}
				if (flag3)
				{
					continue;
				}
				if (vector3.Y + (float)Height <= vector4.Y)
				{
					down = true;
					if ((!(Main.tileSolidTop[Main.tile[i, j].type] && fallThrough) || !(Velocity.Y <= 1f || fall2)) && num6 > vector4.Y)
					{
						num3 = i;
						num4 = j;
						if (num7 < 16)
						{
							num4++;
						}
						if (num3 != num && !flag2)
						{
							result.Y = vector4.Y - (vector3.Y + (float)Height) + ((gravDir == -1) ? (-0.01f) : 0f);
							num6 = vector4.Y;
						}
					}
				}
				else if (vector3.X + (float)Width <= vector4.X && !Main.tileSolidTop[Main.tile[i, j].type])
				{
					if (i >= 1 && Main.tile[i - 1, j] == null)
					{
						Main.tile[i - 1, j] = new Tile();
					}
					if (!hoik || i < 1 || (Main.tile[i - 1, j].slope() != 2 && Main.tile[i - 1, j].slope() != 4))
					{
						num = i;
						num2 = j;
						if (num2 != num4)
						{
							result.X = vector4.X - (vector3.X + (float)Width);
						}
						if (num3 == num)
						{
							result.Y = vector.Y;
						}
					}
				}
				else if (vector3.X >= vector4.X + 16f && !Main.tileSolidTop[Main.tile[i, j].type])
				{
					if (Main.tile[i + 1, j] == null)
					{
						Main.tile[i + 1, j] = new Tile();
					}
					if (!hoik || (Main.tile[i + 1, j].slope() != 1 && Main.tile[i + 1, j].slope() != 3))
					{
						num = i;
						num2 = j;
						if (num2 != num4)
						{
							result.X = vector4.X + 16f - vector3.X;
						}
						if (num3 == num)
						{
							result.Y = vector.Y;
						}
					}
				}
				else if (vector3.Y >= vector4.Y + (float)num7 && !Main.tileSolidTop[Main.tile[i, j].type])
				{
					up = true;
					num3 = i;
					num4 = j;
					result.Y = vector4.Y + (float)num7 - vector3.Y + ((gravDir == 1) ? 0.01f : 0f);
					if (num4 == num2)
					{
						result.X = vector.X;
					}
				}
			}
		}
		return result;
	
	}
	public static bool SolidCollision(Vector2 Position, int Width, int Height)
{
	
		int value = (int)(Position.X / 16f) - 1;
		int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
		int value3 = (int)(Position.Y / 16f) - 1;
		int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
		int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
		value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
		value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 40);
		value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 40);
		Vector2 vector = default(Vector2);
		for (int i = num; i < value2; i++)
		{
			for (int j = value3; j < value4; j++)
			{
				if (Main.tile[i, j] != null && !Main.tile[i, j].inActive() && Main.tile[i, j].active() && Main.tileSolid[Main.tile[i, j].type] && !Main.tileSolidTop[Main.tile[i, j].type])
				{
					vector.X = i * 16;
					vector.Y = j * 16;
					int num2 = 16;
					if (Main.tile[i, j].halfBrick())
					{
						vector.Y += 8f;
						num2 -= 8;
					}
					if (Position.X + (float)Width > vector.X && Position.X < vector.X + 16f && Position.Y + (float)Height > vector.Y && Position.Y < vector.Y + (float)num2)
					{
						return true;
					}
				}
			}
		}
		return false;
	
	}
	public static bool SolidCollision(Vector2 Position, int Width, int Height, bool acceptTopSurfaces)
{
	
		int value = (int)(Position.X / 16f) - 1;
		int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
		int value3 = (int)(Position.Y / 16f) - 1;
		int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
		int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
		value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
		value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 40);
		value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 40);
		Vector2 vector = default(Vector2);
		for (int i = num; i < value2; i++)
		{
			for (int j = value3; j < value4; j++)
			{
				Tile tile = Main.tile[i, j];
				if (tile == null || !tile.active() || tile.inActive())
				{
					continue;
				}
				bool flag = Main.tileSolid[tile.type] && !Main.tileSolidTop[tile.type];
				if (acceptTopSurfaces)
				{
					flag = ((!TileID.Sets.Platforms[tile.type]) ? (flag | (Main.tileSolidTop[tile.type] && tile.frameY == 0)) : (flag | WorldGen.PlatformProperTopFrame(tile.frameX)));
				}
				if (flag)
				{
					vector.X = i * 16;
					vector.Y = j * 16;
					int num2 = 16;
					if (tile.halfBrick())
					{
						vector.Y += 8f;
						num2 -= 8;
					}
					if (Position.X + (float)Width > vector.X && Position.X < vector.X + 16f && Position.Y + (float)Height > vector.Y && Position.Y < vector.Y + (float)num2)
					{
						return true;
					}
				}
			}
		}
		return false;
	
	}
	public static bool AnyHurtingTiles(Vector2 Position, int Width, int Height)
{
	
		return HurtTiles(Position, Width, Height, null).type >= 0;
	
	}
	public static HurtTile HurtTiles(Vector2 Position, int Width, int Height, Player player)
{
	
		int num = (int)(Position.X / 16f) - 1;
		int num2 = (int)((Position.X + (float)Width) / 16f) + 2;
		int num3 = (int)(Position.Y / 16f) - 1;
		int num4 = (int)((Position.Y + (float)Height) / 16f) + 2;
		if (num < 0)
		{
			num = 0;
		}
		if (num2 > Main.maxTilesX)
		{
			num2 = Main.maxTilesX;
		}
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num4 > Main.maxTilesY - 40)
		{
			num4 = Main.maxTilesY - 40;
		}
		Vector2 vector = default(Vector2);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				Tile tile = Main.tile[i, j];
				if (tile == null || tile.inActive() || !tile.active())
				{
					continue;
				}
				vector.X = i * 16;
				vector.Y = j * 16;
				int num5 = 16;
				if (tile.halfBrick())
				{
					vector.Y += 8f;
					num5 -= 8;
				}
				int num6 = 0;
				if (TileID.Sets.Suffocate[tile.type])
				{
					num6 = 2;
				}
				if (Position.X + (float)Width - (float)num6 < vector.X || Position.X + (float)num6 > vector.X + 16f || Position.Y + (float)Height - (float)num6 < vector.Y - 0.5f || Position.Y + (float)num6 > vector.Y + (float)num5 + 0.5f || !CanTileHurt(tile.type, i, j, player))
				{
					continue;
				}
				if (tile.slope() > 0)
				{
					if (num6 > 0)
					{
						continue;
					}
					int num7 = 0;
					if (tile.rightSlope() && Position.X > vector.X)
					{
						num7++;
					}
					if (tile.leftSlope() && Position.X + (float)Width < vector.X + 16f)
					{
						num7++;
					}
					if (tile.bottomSlope() && Position.Y > vector.Y)
					{
						num7++;
					}
					if (tile.topSlope() && Position.Y + (float)Height < vector.Y + (float)num5)
					{
						num7++;
					}
					if (num7 == 2)
					{
						continue;
					}
				}
				return new HurtTile
				{
					type = tile.type,
					x = i,
					y = j
				};
			}
		}
		return new HurtTile
		{
			type = -1
		};
	
	}
	public static bool CanTileHurt(ushort type, int i, int j, Player player)
{
	
		if (type == 230 && !Main.getGoodWorld)
		{
			return false;
		}
		if (type == 80 && !Main.dontStarveWorld)
		{
			return false;
		}
		if (TileID.Sets.TouchDamageBleeding[type] || TileID.Sets.Suffocate[type] || TileID.Sets.TouchDamageImmediate[type] > 0)
		{
			return true;
		}
		if (TileID.Sets.TouchDamageHot[type] && (player == null || !player.fireWalk))
		{
			return true;
		}
		return false;
	
	}
	public static bool SolidTiles(Vector2 position, int width, int height)
{
	
		return SolidTiles((int)(position.X / 16f), (int)((position.X + (float)width) / 16f), (int)(position.Y / 16f), (int)((position.Y + (float)height) / 16f));
	
	}
	public static bool SolidTiles(int startX, int endX, int startY, int endY)
{
	
		if (startX < 0)
		{
			return true;
		}
		if (endX >= Main.maxTilesX)
		{
			return true;
		}
		if (startY < 0)
		{
			return true;
		}
		if (endY >= Main.maxTilesY - 40)
		{
			return true;
		}
		for (int i = startX; i < endX + 1; i++)
		{
			for (int j = startY; j < endY + 1; j++)
			{
				if (Main.tile[i, j] == null)
				{
					return false;
				}
				if (Main.tile[i, j].active() && !Main.tile[i, j].inActive() && Main.tileSolid[Main.tile[i, j].type] && !Main.tileSolidTop[Main.tile[i, j].type])
				{
					return true;
				}
			}
		}
		return false;
	
	}
	public static bool SolidTiles(Vector2 position, int width, int height, bool allowTopSurfaces)
{
	
		return SolidTiles((int)(position.X / 16f), (int)((position.X + (float)width) / 16f), (int)(position.Y / 16f), (int)((position.Y + (float)height) / 16f), allowTopSurfaces);
	
	}
	public static bool SolidTiles(int startX, int endX, int startY, int endY, bool allowTopSurfaces)
{
	
		if (startX < 0)
		{
			return true;
		}
		if (endX >= Main.maxTilesX)
		{
			return true;
		}
		if (startY < 0)
		{
			return true;
		}
		if (endY >= Main.maxTilesY - 40)
		{
			return true;
		}
		for (int i = startX; i < endX + 1; i++)
		{
			for (int j = startY; j < endY + 1; j++)
			{
				Tile tile = Main.tile[i, j];
				if (tile == null)
				{
					return false;
				}
				if (tile.active() && !Main.tile[i, j].inActive())
				{
					ushort type = tile.type;
					bool flag = Main.tileSolid[type] && !Main.tileSolidTop[type];
					if (allowTopSurfaces)
					{
						flag |= Main.tileSolidTop[type] && tile.frameY == 0;
					}
					if (flag)
					{
						return true;
					}
				}
			}
		}
		return false;
	
	}
}
