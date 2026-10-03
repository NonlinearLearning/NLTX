using NSSLC.WorldGeneration.GameContent;
using System.Threading;
using NSSLC.WorldGeneration.GameContent.Generation.Dungeon;
using System;
using System.Collections.Generic;
using NSSLC.WorldGeneration.Geometry;
using NSSLC.WorldGeneration.Utilities;
using NSSLC.WorldGeneration.DataStructures;
namespace NSSLC.WorldGeneration;
public static partial class Utils {
  public delegate bool TileActionAttempt(int x, int y);
  public const int MaxFloatInt = 16777216;
	public static double Lerp(double value1, double value2, double amount)
{
	
		return value1 + (value2 - value1) * amount;
	
	}
	public static float UnclampedSmoothStep(float min, float max, float x)
{
	
		return (x - min) / (max - min);
	
	}
	public static double UnclampedSmoothStep(double min, double max, double x)
{
	
		return (x - min) / (max - min);
	
	}
	public static void Swap<T>(ref T t1, ref T t2)
{
	
		T val = t1;
		t1 = t2;
		t2 = val;
	
	}
	public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
{
	
		if (value.CompareTo(max) > 0)
		{
			return max;
		}
		if (value.CompareTo(min) < 0)
		{
			return min;
		}
		return value;
	
	}
	public static Rectangle Clamp(Rectangle r, Rectangle bounds)
{
	
		return new Rectangle(Clamp(r.X, bounds.Left, bounds.Right - r.Width), Clamp(r.Y, bounds.Top, bounds.Bottom - r.Height), r.Width, r.Height);
	
	}
	public static float MultiLerp(float percent, params float[] floats)
{
	
		float num = 1f / ((float)floats.Length - 1f);
		float num2 = num;
		int num3 = 0;
		while (percent / num2 > 1f && num3 < floats.Length - 2)
		{
			num2 += num;
			num3++;
		}
		return MathHelper.Lerp(floats[num3], floats[num3 + 1], (percent - num * (float)num3) / num);
	
	}
	public static Color MultiLerp(float percent, params Color[] colors)
{
	
		float num = 1f / ((float)colors.Length - 1f);
		float num2 = num;
		int num3 = 0;
		while (percent / num2 > 1f && num3 < colors.Length - 2)
		{
			num2 += num;
			num3++;
		}
		return Color.Lerp(colors[num3], colors[num3 + 1], (percent - num * (float)num3) / num);
	
	}
	public static float WrappedLerp(float value1, float value2, float percent)
{
	
		float num = percent * 2f;
		if (num > 1f)
		{
			num = 2f - num;
		}
		return MathHelper.Lerp(value1, value2, num);
	
	}
	public static float GetLerpValue(float from, float to, float t, bool clamped = false)
{
	
		if (clamped)
		{
			if (from < to)
			{
				if (t < from)
				{
					return 0f;
				}
				if (t > to)
				{
					return 1f;
				}
			}
			else
			{
				if (t < to)
				{
					return 1f;
				}
				if (t > from)
				{
					return 0f;
				}
			}
		}
		return (t - from) / (to - from);
	
	}
	public static float Remap(float fromValue, float fromMin, float fromMax, float toMin, float toMax, bool clamped = true)
{
	
		return MathHelper.Lerp(toMin, toMax, GetLerpValue(fromMin, fromMax, fromValue, clamped));
	
	}
	public static double Remap(double fromValue, double fromMin, double fromMax, double toMin, double toMax, bool clamped = true)
{
	
		return Lerp(toMin, toMax, GetLerpValue(fromMin, fromMax, fromValue, clamped));
	
	}
	public static double GetLerpValue(double from, double to, double t, bool clamped = false)
{
	
		if (clamped)
		{
			if (from < to)
			{
				if (t < from)
				{
					return 0.0;
				}
				if (t > to)
				{
					return 1.0;
				}
			}
			else
			{
				if (t < to)
				{
					return 1.0;
				}
				if (t > from)
				{
					return 0.0;
				}
			}
		}
		return (t - from) / (to - from);
	
	}
	public static T Max<T>(params T[] args) where T : IComparable
{
	
		T result = args[0];
		for (int i = 1; i < args.Length; i++)
		{
			object obj = args[i];
			if (result.CompareTo(obj) < 0)
			{
				result = args[i];
			}
		}
		return result;
	
	}
	public static Rectangle CenteredRectangle(Vector2 center, Vector2 size)
{
	
		return new Rectangle((int)(center.X - size.X / 2f), (int)(center.Y - size.Y / 2f), (int)size.X, (int)size.Y);
	
	}
	public static Rectangle CenteredRectangle(Point center, Point size)
{
	
		return new Rectangle(center.X - size.X / 2, center.Y - size.Y / 2, size.X, size.Y);
	
	}
	public static bool FloatIntersect(float r1StartX, float r1StartY, float r1Width, float r1Height, float r2StartX, float r2StartY, float r2Width, float r2Height)
{
	
		if (r1StartX > r2StartX + r2Width || r1StartY > r2StartY + r2Height || r1StartX + r1Width < r2StartX || r1StartY + r1Height < r2StartY)
		{
			return false;
		}
		return true;
	
	}
	public static bool DoubleIntersect(double r1StartX, double r1StartY, double r1Width, double r1Height, double r2StartX, double r2StartY, double r2Width, double r2Height)
{
	
		if (r1StartX > r2StartX + r2Width || r1StartY > r2StartY + r2Height || r1StartX + r1Width < r2StartX || r1StartY + r1Height < r2StartY)
		{
			return false;
		}
		return true;
	
	}
	public static bool LineSegmentsIntersect(Vector2D start1, Vector2D end1, Vector2D start2, Vector2D end2)
{
	
		Vector2D vector2D = end1 - start1;
		Vector2D vector2D2 = end2 - start2;
		double num = Vector2D.Cross(vector2D, vector2D2);
		if (num == 0.0)
		{
			return false;
		}
		Vector2D vector2D3 = start2 - start1;
		_ = Vector2D.Cross(vector2D3, vector2D) / num;
		double num2 = Vector2D.Cross(vector2D3, vector2D) / num;
		double num3 = Vector2D.Cross(vector2D3, vector2D) / num;
		if (0.0 <= num2 && num2 <= 1.0 && 0.0 <= num3)
		{
			return num3 <= 1.0;
		}
		return false;
	
	}
	public static float NextFloat(this UnifiedRandom r)
{
	
		return (float)r.NextDouble();
	
	}
	public static float NextFloat(this UnifiedRandom random, FloatRange range)
{
	
		return random.NextFloat() * (range.Maximum - range.Minimum) + range.Minimum;
	
	}
	public static T NextFromList<T>(this UnifiedRandom random, params T[] objs)
{
	
		return objs[random.Next(objs.Length)];
	
	}
	public static int Next(this UnifiedRandom random, IntRange range)
{
	
		return random.Next(range.Minimum, range.Maximum + 1);
	
	}
	public static Point NextFromRectangle(this UnifiedRandom r, Rectangle rect)
{
	
		return new Point(r.Next(rect.Left, rect.Right), r.Next(rect.Top, rect.Bottom));
	
	}
	public static Vector2 NextVector2Unit(this UnifiedRandom r, float startRotation = 0f, float rotationRange = (float)Math.PI * 2f)
{
	
		return (startRotation + rotationRange * r.NextFloat()).ToRotationVector2();
	
	}
	public static Vector2 NextVector2Circular(this UnifiedRandom r, float circleHalfWidth, float circleHalfHeight)
{
	
		return r.NextVector2Unit() * new Vector2(circleHalfWidth, circleHalfHeight) * r.NextFloat();
	
	}
	public static Vector2 NextVector2CircularEdge(this UnifiedRandom r, float circleHalfWidth, float circleHalfHeight)
{
	
		return r.NextVector2Unit() * new Vector2(circleHalfWidth, circleHalfHeight);
	
	}
	public static Vector2D NextVector2DUnit(this UnifiedRandom r, double startRotation = 0.0, double rotationRange = 6.2831854820251465)
{
	
		return (startRotation + rotationRange * r.NextDouble()).ToRotationVector2D();
	
	}
	public static Vector2D NextVector2DCircular(this UnifiedRandom r, double circleHalfWidth, double circleHalfHeight)
{
	
		return r.NextVector2DUnit() * new Vector2D(circleHalfWidth, circleHalfHeight) * r.NextDouble();
	
	}
	public static Vector2D NextVector2DCircularEdge(this UnifiedRandom r, double circleHalfWidth, double circleHalfHeight)
{
	
		return r.NextVector2DUnit() * new Vector2D(circleHalfWidth, circleHalfHeight);
	
	}
	public static Vector2 TopLeft(this Rectangle r)
{
	
		return new Vector2(r.X, r.Y);
	
	}
	public static Vector2 TopRight(this Rectangle r)
{
	
		return new Vector2(r.X + r.Width, r.Y);
	
	}
	public static Vector2 BottomLeft(this Rectangle r)
{
	
		return new Vector2(r.X, r.Y + r.Height);
	
	}
	public static Vector2 BottomRight(this Rectangle r)
{
	
		return new Vector2(r.X + r.Width, r.Y + r.Height);
	
	}
	public static Vector2D TopLeftDouble(this Rectangle r)
{
	
		return new Vector2D(r.X, r.Y);
	
	}
	public static Vector2D TopRightDouble(this Rectangle r)
{
	
		return new Vector2D(r.X + r.Width, r.Y);
	
	}
	public static Vector2D BottomLeftDouble(this Rectangle r)
{
	
		return new Vector2D(r.X, r.Y + r.Height);
	
	}
	public static Vector2D BottomRightDouble(this Rectangle r)
{
	
		return new Vector2D(r.X + r.Width, r.Y + r.Height);
	
	}
	public static float Distance(this Rectangle r, Vector2 point)
{
	
		if (FloatIntersect(r.Left, r.Top, r.Width, r.Height, point.X, point.Y, 0f, 0f))
		{
			return 0f;
		}
		if (point.X >= (float)r.Left && point.X <= (float)r.Right)
		{
			if (point.Y < (float)r.Top)
			{
				return (float)r.Top - point.Y;
			}
			return point.Y - (float)r.Bottom;
		}
		if (point.Y >= (float)r.Top && point.Y <= (float)r.Bottom)
		{
			if (point.X < (float)r.Left)
			{
				return (float)r.Left - point.X;
			}
			return point.X - (float)r.Right;
		}
		if (point.X < (float)r.Left)
		{
			if (point.Y < (float)r.Top)
			{
				return Vector2.Distance(point, r.TopLeft());
			}
			return Vector2.Distance(point, r.BottomLeft());
		}
		if (point.Y < (float)r.Top)
		{
			return Vector2.Distance(point, r.TopRight());
		}
		return Vector2.Distance(point, r.BottomRight());
	
	}
	public static double Distance(this Rectangle r, Vector2D point)
{
	
		if (DoubleIntersect(r.Left, r.Top, r.Width, r.Height, point.X, point.Y, 0.0, 0.0))
		{
			return 0.0;
		}
		if (point.X >= (double)r.Left && point.X <= (double)r.Right)
		{
			if (point.Y < (double)r.Top)
			{
				return (double)r.Top - point.Y;
			}
			return point.Y - (double)r.Bottom;
		}
		if (point.Y >= (double)r.Top && point.Y <= (double)r.Bottom)
		{
			if (point.X < (double)r.Left)
			{
				return (double)r.Left - point.X;
			}
			return point.X - (double)r.Right;
		}
		if (point.X < (double)r.Left)
		{
			if (point.Y < (double)r.Top)
			{
				return Vector2D.Distance(point, r.TopLeftDouble());
			}
			return Vector2D.Distance(point, r.BottomLeftDouble());
		}
		if (point.Y < (double)r.Top)
		{
			return Vector2D.Distance(point, r.TopRightDouble());
		}
		return Vector2D.Distance(point, r.BottomRightDouble());
	
	}
	public static Rectangle Modified(this Rectangle r, int x, int y, int w, int h)
{
	
		return new Rectangle(r.X + x, r.Y + y, r.Width + w, r.Height + h);
	
	}
	public static float ToRotation(this Vector2 v)
{
	
		return (float)Math.Atan2(v.Y, v.X);
	
	}
	public static double ToRotation(this Vector2D v)
{
	
		return Math.Atan2(v.Y, v.X);
	
	}
	public static Vector2 ToRotationVector2(this float f)
{
	
		return new Vector2((float)Math.Cos(f), (float)Math.Sin(f));
	
	}
	public static Vector2D ToRotationVector2D(this double f)
{
	
		return new Vector2D(Math.Cos(f), Math.Sin(f));
	
	}
	public static Vector2 RotatedBy(this Vector2 spinningpoint, double radians, Vector2 center = default(Vector2))
{
	
		float num = (float)Math.Cos(radians);
		float num2 = (float)Math.Sin(radians);
		Vector2 vector = spinningpoint - center;
		Vector2 result = center;
		result.X += vector.X * num - vector.Y * num2;
		result.Y += vector.X * num2 + vector.Y * num;
		return result;
	
	}
	public static Vector2D RotatedBy(this Vector2D spinningpoint, double radians, Vector2D center = default(Vector2D))
{
	
		double num = Math.Cos(radians);
		double num2 = Math.Sin(radians);
		Vector2D vector2D = spinningpoint - center;
		Vector2D result = center;
		result.X += vector2D.X * num - vector2D.Y * num2;
		result.Y += vector2D.X * num2 + vector2D.Y * num;
		return result;
	
	}
	public static Vector2 RotatedByRandom(this Vector2 spinninpoint, double maxRadians)
{
	
		return spinninpoint.RotatedBy(Main.rand.NextDouble() * maxRadians - Main.rand.NextDouble() * maxRadians);
	
	}
	public static bool HasNaNs(this Vector2 vec)
{
	
		if (!float.IsNaN(vec.X))
		{
			return float.IsNaN(vec.Y);
		}
		return true;
	
	}
	public static Vector2 ToVector2(this Point p)
{
	
		return new Vector2(p.X, p.Y);
	
	}
	public static Vector2 ToVector2(this Point16 p)
{
	
		return new Vector2(p.X, p.Y);
	
	}
	public static Vector2D ToVector2D(this Point p)
{
	
		return new Vector2D(p.X, p.Y);
	
	}
	public static Vector2D ToVector2D(this Point16 p)
{
	
		return new Vector2D(p.X, p.Y);
	
	}
	public static Vector2 ToWorldCoordinates(this Point p, float autoAddX = 8f, float autoAddY = 8f)
{
	
		return p.ToVector2() * 16f + new Vector2(autoAddX, autoAddY);
	
	}
	public static Vector2 ToWorldCoordinates(this Point16 p, float autoAddX = 8f, float autoAddY = 8f)
{
	
		return p.ToVector2() * 16f + new Vector2(autoAddX, autoAddY);
	
	}
	public static Point ToTileCoordinates(this Vector2 vec)
{
	
		return new Point((int)vec.X >> 4, (int)vec.Y >> 4);
	
	}
	public static Point ToTileCoordinates(this Vector2D vec)
{
	
		return new Point((int)vec.X >> 4, (int)vec.Y >> 4);
	
	}
	public static Point ToPoint(this Vector2 v)
{
	
		return new Point((int)v.X, (int)v.Y);
	
	}
	public static Point ToPoint(this Vector2D v)
{
	
		return new Point((int)v.X, (int)v.Y);
	
	}
	public static Vector2 ToVector2(this Vector2D v)
{
	
		return new Vector2((float)v.X, (float)v.Y);
	
	}
	public static Vector2D ToVector2D(this Vector2 v)
{
	
		return new Vector2D(v.X, v.Y);
	
	}
	public static Vector2 SafeNormalize(this Vector2 v, Vector2 defaultValue)
{
	
		if (v == Vector2.Zero || v.HasNaNs())
		{
			return defaultValue;
		}
		return Vector2.Normalize(v);
	
	}
	public static Vector2D SafeNormalize(this Vector2D v, Vector2D defaultValue)
{
	
		if (v == Vector2D.Zero)
		{
			return defaultValue;
		}
		return Vector2D.Normalize(v);
	
	}
	public static Vector2 ClosestPointOnLine(this Vector2 P, Vector2 A, Vector2 B)
{
	
		Vector2 value = P - A;
		Vector2 vector = B - A;
		float num = vector.LengthSquared();
		float num2 = Vector2.Dot(value, vector) / num;
		if (num2 < 0f)
		{
			return A;
		}
		if (num2 > 1f)
		{
			return B;
		}
		return A + vector * num2;
	
	}
	public static Vector2D ClosestPointOnLine(this Vector2D P, Vector2D A, Vector2D B)
{
	
		Vector2D value = P - A;
		Vector2D vector2D = B - A;
		double num = vector2D.LengthSquared();
		double num2 = Vector2D.Dot(value, vector2D) / num;
		if (num2 < 0.0)
		{
			return A;
		}
		if (num2 > 1.0)
		{
			return B;
		}
		return A + vector2D * num2;
	
	}
	public static float Distance(this Vector2 Origin, Vector2 Target)
{
	
		return Vector2.Distance(Origin, Target);
	
	}
	public static double Distance(this Vector2D Origin, Vector2D Target)
{
	
		return Vector2D.Distance(Origin, Target);
	
	}
	public static bool PlotLine(Point16 p0, Point16 p1, TileActionAttempt plot, bool jump = true)
{
	
		return PlotLine(p0.X, p0.Y, p1.X, p1.Y, plot, jump);
	
	}
	public static bool PlotLine(Point p0, Point p1, TileActionAttempt plot, bool jump = true)
{
	
		return PlotLine(p0.X, p0.Y, p1.X, p1.Y, plot, jump);
	
	}
	private static bool PlotLine(int x0, int y0, int x1, int y1, TileActionAttempt plot, bool jump = true)
{
	
		if (x0 == x1 && y0 == y1)
		{
			return plot(x0, y0);
		}
		bool flag = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if (flag)
		{
			Swap(ref x0, ref y0);
			Swap(ref x1, ref y1);
		}
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = num / 2;
		int num4 = y0;
		int num5 = ((x0 < x1) ? 1 : (-1));
		int num6 = ((y0 < y1) ? 1 : (-1));
		for (int i = x0; i != x1; i += num5)
		{
			if (flag)
			{
				if (!plot(num4, i))
				{
					return false;
				}
			}
			else if (!plot(i, num4))
			{
				return false;
			}
			num3 -= num2;
			if (num3 >= 0)
			{
				continue;
			}
			num4 += num6;
			if (!jump)
			{
				if (flag)
				{
					if (!plot(num4, i))
					{
						return false;
					}
				}
				else if (!plot(i, num4))
				{
					return false;
				}
			}
			num3 += num;
		}
		return true;
	
	}
	public static bool PlotTileLine(Vector2 start, Vector2 end, float width, TileActionAttempt plot)
{
	
		return PlotTileLine(start.ToVector2D(), end.ToVector2D(), width, plot);
	
	}
	public static bool PlotTileLine(Vector2D start, Vector2D end, double width, TileActionAttempt plot)
{
	
		double num = width / 2.0;
		Vector2D vector2D = end - start;
		Vector2D vector2D2 = vector2D / vector2D.Length();
		Vector2D vector2D3 = new Vector2D(0.0 - vector2D2.Y, vector2D2.X) * num;
		Point point = (start - vector2D3).ToTileCoordinates();
		Point point2 = (start + vector2D3).ToTileCoordinates();
		Point point3 = start.ToTileCoordinates();
		Point point4 = end.ToTileCoordinates();
		Point lineMinOffset = new Point(point.X - point3.X, point.Y - point3.Y);
		Point lineMaxOffset = new Point(point2.X - point3.X, point2.Y - point3.Y);
		return PlotLine(point3.X, point3.Y, point4.X, point4.Y, (int x, int y) => PlotLine(x + lineMinOffset.X, y + lineMinOffset.Y, x + lineMaxOffset.X, y + lineMaxOffset.Y, plot, jump: false));
	
	}
	public static bool PlotTileTale(Vector2D start, Vector2D end, double width, TileActionAttempt plot)
{
	
		double halfWidth = width / 2.0;
		Vector2D vector2D = end - start;
		Vector2D vector2D2 = vector2D / vector2D.Length();
		Vector2D perpOffset = new Vector2D(0.0 - vector2D2.Y, vector2D2.X);
		Point pointStart = start.ToTileCoordinates();
		Point point = end.ToTileCoordinates();
		int length = 0;
		PlotLine(pointStart.X, pointStart.Y, point.X, point.Y, delegate
		{
			length++;
			return true;
		});
		length--;
		int curLength = 0;
		return PlotLine(pointStart.X, pointStart.Y, point.X, point.Y, delegate(int x, int y)
		{
			double num = 1.0 - (double)curLength / (double)length;
			curLength++;
			Point point2 = (start - perpOffset * halfWidth * num).ToTileCoordinates();
			Point point3 = (start + perpOffset * halfWidth * num).ToTileCoordinates();
			Point point4 = new Point(point2.X - pointStart.X, point2.Y - pointStart.Y);
			Point point5 = new Point(point3.X - pointStart.X, point3.Y - pointStart.Y);
			return PlotLine(x + point4.X, y + point4.Y, x + point5.X, y + point5.Y, plot, jump: false);
		});
	
	}
	public static Vector2 RandomVector2(UnifiedRandom random, float min, float max)
{
	
		return new Vector2((max - min) * (float)random.NextDouble() + min, (max - min) * (float)random.NextDouble() + min);
	
	}
	public static Vector2D RandomVector2D(UnifiedRandom random, double min, double max)
{
	
		return new Vector2D((max - min) * random.NextDouble() + min, (max - min) * random.NextDouble() + min);
	
	}
	public static T SelectRandom<T>(UnifiedRandom random, params T[] choices)
{
	
		return choices[random.Next(choices.Length)];
	
	}
	public static bool TryOperateInLock(object _lock, Action action)
{
	
		if (!Monitor.TryEnter(_lock))
		{
			return false;
		}
		try
		{
			action();
			return true;
		}
		finally
		{
			Monitor.Exit(_lock);
		}
	
	}
public class RandomTeleportationAttemptSettings
	{
		public Vector2 teleporteeSize;

		public Vector2 teleporteeVelocity;

		public float teleporteeGravityDirection;

		public bool mostlySolidFloor;

		public bool avoidLava;

		public bool avoidAnyLiquid;

		public bool avoidHurtTiles;

		public bool avoidWalls;

		public int attemptsBeforeGivingUp;

		public int maximumFallDistanceFromOrignalPoint;

		public bool strictRange;

		public int[] tilesToAvoid;

		public int tilesToAvoidRange;

		public bool allowSolidTopFloor;

		public Func<Tile, int, int, bool> specializedConditions;
	}
	public static Vector2 CheckForGoodTeleportationSpot(ref bool canSpawn, int teleportStartX, int teleportRangeX, int teleportStartY, int teleportRangeY, RandomTeleportationAttemptSettings settings)
{
	
		int num = (int)settings.teleporteeSize.X;
		int num2 = (int)settings.teleporteeSize.Y;
		Vector2 teleporteeVelocity = settings.teleporteeVelocity;
		float teleporteeGravityDirection = settings.teleporteeGravityDirection;
		Rectangle rectangle = new Rectangle(teleportStartX, teleportStartY, teleportRangeX, teleportRangeY);
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = num;
		Vector2 vector = new Vector2(num4, num5) * 16f + new Vector2(-num6 / 2 + 8, -num2);
		while (!canSpawn && num3 < settings.attemptsBeforeGivingUp)
		{
			num3++;
			num4 = teleportStartX + Main.rand.Next(teleportRangeX);
			num5 = teleportStartY + Main.rand.Next(teleportRangeY);
			int num7 = 45;
			num4 = (int)MathHelper.Clamp(num4, num7, Main.maxTilesX - num7);
			num5 = (int)MathHelper.Clamp(num5, num7, Main.maxTilesY - num7);
			if (settings.strictRange && !rectangle.Contains(new Point(num4, num5)))
			{
				continue;
			}
			vector = new Vector2(num4, num5) * 16f + new Vector2(-num6 / 2 + 8, -num2);
			if (Collision.SolidCollision(vector, num6, num2))
			{
				continue;
			}
			if (Main.tile[num4, num5] == null)
			{
				Main.tile[num4, num5] = new Tile();
			}
			Tile tile = Main.tile[num4, num5];
			if ((settings.avoidWalls && tile.wall > 0) || (tile.wall == 87 && (double)num5 > Main.worldSurface && !NPC.downedPlantBoss) || (Main.wallDungeon[tile.wall] && (double)num5 > Main.worldSurface && !NPC.downedBoss3) || !CheckForGoodTeleportationSpot_CheckNoInvalidTiles(num4, num5, settings))
			{
				continue;
			}
			bool flag = false;
			int num8 = 0;
			while (num8 < settings.maximumFallDistanceFromOrignalPoint)
			{
				if (settings.strictRange && !rectangle.Contains(new Point(num4, num5 + num8)))
				{
					flag = true;
					break;
				}
				if (Main.tile[num4, num5 + num8] == null)
				{
					Main.tile[num4, num5 + num8] = new Tile();
				}
				Tile tile2 = Main.tile[num4, num5 + num8];
				vector = new Vector2(num4, num5 + num8) * 16f + new Vector2(-num6 / 2 + 8, -num2);
				Collision.SlopeCollision(vector, teleporteeVelocity, num6, num2, teleporteeGravityDirection);
				if (!Collision.SolidCollision(vector, num6, num2 + 1, settings.allowSolidTopFloor))
				{
					num8++;
					continue;
				}
				if (tile2.active() && !tile2.inActive() && Main.tileSolid[tile2.type])
				{
					break;
				}
				num8++;
			}
			if (flag)
			{
				continue;
			}
			int num9 = (int)vector.X / 16;
			int num10 = (int)vector.Y / 16;
			if (!CheckForGoodTeleportationSpot_CheckNoInvalidTiles(num9, num10, settings))
			{
				continue;
			}
			int num11 = (int)(vector.X + (float)num6 * 0.5f) / 16;
			int num12 = (int)(vector.Y + (float)num2) / 16;
			Tile tileSafely = Framing.GetTileSafely(num9, num10);
			Tile tileSafely2 = Framing.GetTileSafely(num11, num12);
			if ((settings.specializedConditions != null && !settings.specializedConditions(tileSafely2, num11, num12)) || (settings.avoidAnyLiquid && tileSafely2.liquid > 0))
			{
				continue;
			}
			if (settings.mostlySolidFloor)
			{
				Tile tileSafely3 = Framing.GetTileSafely(num11 - 1, num12);
				Tile tileSafely4 = Framing.GetTileSafely(num11 + 1, num12);
				bool flag2 = false;
				bool flag3 = false;
				if (settings.allowSolidTopFloor)
				{
					flag2 = !tileSafely3.inActive() && WorldGen.SolidTileAllowBottomSlope(num11 - 1, num12);
					flag3 = !tileSafely4.inActive() && WorldGen.SolidTileAllowBottomSlope(num11 + 1, num12);
				}
				else
				{
					flag2 = tileSafely3.active() && !tileSafely3.inActive() && Main.tileSolid[tileSafely3.type] && !Main.tileSolidTop[tileSafely3.type];
					flag3 = tileSafely4.active() && !tileSafely4.inActive() && Main.tileSolid[tileSafely4.type] && !Main.tileSolidTop[tileSafely4.type];
				}
				if (!flag2 && !flag3)
				{
					continue;
				}
			}
			if ((settings.avoidWalls && tileSafely.wall > 0) || (settings.avoidAnyLiquid && Collision.WetCollision(vector, num6, num2)) || (settings.avoidLava && Collision.LavaCollision(vector, num6, num2)) || (settings.avoidHurtTiles && Collision.AnyHurtingTiles(vector, num6, num2)) || Collision.SolidCollision(vector, num6, num2, settings.allowSolidTopFloor) || num8 >= settings.maximumFallDistanceFromOrignalPoint - 1)
			{
				continue;
			}
			Vector2 vector2 = Vector2.UnitX * 16f;
			if (Collision.TileCollision(vector - vector2, vector2, num, num2, fallThrough: false, fall2: false, (int)teleporteeGravityDirection) != vector2)
			{
				continue;
			}
			vector2 = -Vector2.UnitX * 16f;
			if (Collision.TileCollision(vector - vector2, vector2, num, num2, fallThrough: false, fall2: false, (int)teleporteeGravityDirection) != vector2)
			{
				continue;
			}
			vector2 = Vector2.UnitY * 16f;
			if (!(Collision.TileCollision(vector - vector2, vector2, num, num2, fallThrough: false, fall2: false, (int)teleporteeGravityDirection) != vector2))
			{
				vector2 = -Vector2.UnitY * 16f;
				if (!(Collision.TileCollision(vector - vector2, vector2, num, num2, fallThrough: false, fall2: false, (int)teleporteeGravityDirection) != vector2) && (!Main.dualDungeonsSeed || !UnbreakableWallScan.InsideUnbreakableWalls(new Point(num9, num10))))
				{
					canSpawn = true;
					num5 += num8;
					break;
				}
			}
		}
		return vector;
	
	}
	private static bool CheckForGoodTeleportationSpot_CheckNoInvalidTiles(int tpx, int tpy, RandomTeleportationAttemptSettings settings)
{
	
		if (settings.tilesToAvoidRange > 0 && settings.tilesToAvoid != null)
		{
			int tilesToAvoidRange = settings.tilesToAvoidRange;
			for (int i = -tilesToAvoidRange; i <= tilesToAvoidRange; i++)
			{
				for (int j = -tilesToAvoidRange; j <= tilesToAvoidRange; j++)
				{
					int num = tpx + i;
					int num2 = tpy + j;
					if (!WorldGen.InWorld(num, num2, 2))
					{
						continue;
					}
					Tile tile = Main.tile[num, num2];
					if (tile == null || !tile.active())
					{
						continue;
					}
					ushort type = tile.type;
					for (int k = 0; k < settings.tilesToAvoid.Length; k++)
					{
						if (type == settings.tilesToAvoid[k])
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	
	}
}
