using System;
using NSSLC.WorldGeneration.Geometry;
using NSSLC.WorldGeneration.GameContent;

namespace NSSLC.WorldGeneration;

public static class Minecart
{
	private enum TrackState
	{
		NoTrack = -1,
		AboveTrack,
		OnTrack,
		BelowTrack,
		AboveFront,
		AboveBack,
		OnFront,
		OnBack
	}

	public struct Customization
	{
		public float MinecartTextureWidth;

		public Vector2 WheelOffset;

		public Vector2 MagnetOffset;

		public static Customization Default => new Customization
		{
			MinecartTextureWidth = 50f,
			MagnetOffset = new Vector2(25f, 26f),
			WheelOffset = new Vector2(12f, 0f)
		};
	}

	private const int TotalFrames = 36;

	public const int LeftDownDecoration = 36;

	public const int RightDownDecoration = 37;

	public const int BouncyBumperDecoration = 38;

	public const int RegularBumperDecoration = 39;

	public const int Flag_OnTrack = 0;

	public const int Flag_BouncyBumper = 1;

	public const int Flag_UsedRamp = 2;

	public const int Flag_HitSwitch = 3;

	public const int Flag_BoostLeft = 4;

	public const int Flag_BoostRight = 5;

	private const int NoConnection = -1;

	private const int TopConnection = 0;

	private const int MiddleConnection = 1;

	private const int BottomConnection = 2;

	private const int BumperEnd = -1;

	private const int BouncyEnd = -2;

	private const int RampEnd = -3;

	private const int OpenEnd = -4;

	public const float BoosterSpeed = 4f;

	private const int Type_Normal = 0;

	private const int Type_Pressure = 1;

	private const int Type_Booster = 2;

	private static int[] _leftSideConnection;

	private static int[] _rightSideConnection;

	private static int[] _trackType;

	private static bool[] _boostLeft;

	private static Vector2[] _texturePosition;

	private static short _firstPressureFrame;

	private static short _firstLeftBoostFrame;

	private static short _firstRightBoostFrame;

	private static int[][] _trackSwitchOptions;

	private static int[][] _tileHeight;

	internal static void Initialize()
{
	
		
		_rightSideConnection = new int[36];
		_leftSideConnection = new int[36];
		_trackType = new int[36];
		_boostLeft = new bool[36];
		_texturePosition = new Vector2[40];
		_tileHeight = new int[36][];
		for (int i = 0; i < 36; i++)
		{
			int[] array = new int[8];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = 5;
			}
			_tileHeight[i] = array;
		}
		int num = 0;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = -1;
		_tileHeight[num][0] = -4;
		_tileHeight[num][7] = -4;
		_texturePosition[num] = new Vector2(0f, 0f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = 1;
		_texturePosition[num] = new Vector2(1f, 0f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 1;
		for (int k = 0; k < 4; k++)
		{
			_tileHeight[num][k] = -1;
		}
		_texturePosition[num] = new Vector2(2f, 1f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = -1;
		for (int l = 4; l < 8; l++)
		{
			_tileHeight[num][l] = -1;
		}
		_texturePosition[num] = new Vector2(3f, 1f);
		num++;
		_leftSideConnection[num] = 2;
		_rightSideConnection[num] = 1;
		_tileHeight[num][0] = 1;
		_tileHeight[num][1] = 2;
		_tileHeight[num][2] = 3;
		_tileHeight[num][3] = 3;
		_tileHeight[num][4] = 4;
		_tileHeight[num][5] = 4;
		_texturePosition[num] = new Vector2(0f, 2f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = 2;
		_tileHeight[num][2] = 4;
		_tileHeight[num][3] = 4;
		_tileHeight[num][4] = 3;
		_tileHeight[num][5] = 3;
		_tileHeight[num][6] = 2;
		_tileHeight[num][7] = 1;
		_texturePosition[num] = new Vector2(1f, 2f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = 0;
		_tileHeight[num][4] = 6;
		_tileHeight[num][5] = 6;
		_tileHeight[num][6] = 7;
		_tileHeight[num][7] = 8;
		_texturePosition[num] = new Vector2(0f, 1f);
		num++;
		_leftSideConnection[num] = 0;
		_rightSideConnection[num] = 1;
		_tileHeight[num][0] = 8;
		_tileHeight[num][1] = 7;
		_tileHeight[num][2] = 6;
		_tileHeight[num][3] = 6;
		_texturePosition[num] = new Vector2(1f, 1f);
		num++;
		_leftSideConnection[num] = 0;
		_rightSideConnection[num] = 2;
		for (int m = 0; m < 8; m++)
		{
			_tileHeight[num][m] = 8 - m;
		}
		_texturePosition[num] = new Vector2(0f, 3f);
		num++;
		_leftSideConnection[num] = 2;
		_rightSideConnection[num] = 0;
		for (int n = 0; n < 8; n++)
		{
			_tileHeight[num][n] = n + 1;
		}
		_texturePosition[num] = new Vector2(1f, 3f);
		num++;
		_leftSideConnection[num] = 2;
		_rightSideConnection[num] = -1;
		_tileHeight[num][0] = 1;
		_tileHeight[num][1] = 2;
		for (int num2 = 2; num2 < 8; num2++)
		{
			_tileHeight[num][num2] = -1;
		}
		_texturePosition[num] = new Vector2(4f, 1f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 2;
		_tileHeight[num][6] = 2;
		_tileHeight[num][7] = 1;
		for (int num3 = 0; num3 < 6; num3++)
		{
			_tileHeight[num][num3] = -1;
		}
		_texturePosition[num] = new Vector2(5f, 1f);
		num++;
		_leftSideConnection[num] = 0;
		_rightSideConnection[num] = -1;
		_tileHeight[num][0] = 8;
		_tileHeight[num][1] = 7;
		_tileHeight[num][2] = 6;
		for (int num4 = 3; num4 < 8; num4++)
		{
			_tileHeight[num][num4] = -1;
		}
		_texturePosition[num] = new Vector2(6f, 1f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 0;
		_tileHeight[num][5] = 6;
		_tileHeight[num][6] = 7;
		_tileHeight[num][7] = 8;
		for (int num5 = 0; num5 < 5; num5++)
		{
			_tileHeight[num][num5] = -1;
		}
		_texturePosition[num] = new Vector2(7f, 1f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 1;
		_tileHeight[num][0] = -4;
		_texturePosition[num] = new Vector2(2f, 0f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = -1;
		_tileHeight[num][7] = -4;
		_texturePosition[num] = new Vector2(3f, 0f);
		num++;
		_leftSideConnection[num] = 2;
		_rightSideConnection[num] = -1;
		for (int num6 = 0; num6 < 6; num6++)
		{
			_tileHeight[num][num6] = num6 + 1;
		}
		_tileHeight[num][6] = -3;
		_tileHeight[num][7] = -3;
		_texturePosition[num] = new Vector2(4f, 0f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 2;
		_tileHeight[num][0] = -3;
		_tileHeight[num][1] = -3;
		for (int num7 = 2; num7 < 8; num7++)
		{
			_tileHeight[num][num7] = 8 - num7;
		}
		_texturePosition[num] = new Vector2(5f, 0f);
		num++;
		_leftSideConnection[num] = 0;
		_rightSideConnection[num] = -1;
		for (int num8 = 0; num8 < 6; num8++)
		{
			_tileHeight[num][num8] = 8 - num8;
		}
		_tileHeight[num][6] = -3;
		_tileHeight[num][7] = -3;
		_texturePosition[num] = new Vector2(6f, 0f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 0;
		_tileHeight[num][0] = -3;
		_tileHeight[num][1] = -3;
		for (int num9 = 2; num9 < 8; num9++)
		{
			_tileHeight[num][num9] = num9 + 1;
		}
		_texturePosition[num] = new Vector2(7f, 0f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = -1;
		_tileHeight[num][0] = -4;
		_tileHeight[num][7] = -4;
		_trackType[num] = 1;
		_texturePosition[num] = new Vector2(0f, 4f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = 1;
		_trackType[num] = 1;
		_texturePosition[num] = new Vector2(1f, 4f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 1;
		_tileHeight[num][0] = -4;
		_trackType[num] = 1;
		_texturePosition[num] = new Vector2(0f, 5f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = -1;
		_tileHeight[num][7] = -4;
		_trackType[num] = 1;
		_texturePosition[num] = new Vector2(1f, 5f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 1;
		for (int num10 = 0; num10 < 6; num10++)
		{
			_tileHeight[num][num10] = -2;
		}
		_texturePosition[num] = new Vector2(2f, 2f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = -1;
		for (int num11 = 2; num11 < 8; num11++)
		{
			_tileHeight[num][num11] = -2;
		}
		_texturePosition[num] = new Vector2(3f, 2f);
		num++;
		_leftSideConnection[num] = 2;
		_rightSideConnection[num] = -1;
		_tileHeight[num][0] = 1;
		_tileHeight[num][1] = 2;
		for (int num12 = 2; num12 < 8; num12++)
		{
			_tileHeight[num][num12] = -2;
		}
		_texturePosition[num] = new Vector2(4f, 2f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 2;
		_tileHeight[num][6] = 2;
		_tileHeight[num][7] = 1;
		for (int num13 = 0; num13 < 6; num13++)
		{
			_tileHeight[num][num13] = -2;
		}
		_texturePosition[num] = new Vector2(5f, 2f);
		num++;
		_leftSideConnection[num] = 0;
		_rightSideConnection[num] = -1;
		_tileHeight[num][0] = 8;
		_tileHeight[num][1] = 7;
		_tileHeight[num][2] = 6;
		for (int num14 = 3; num14 < 8; num14++)
		{
			_tileHeight[num][num14] = -2;
		}
		_texturePosition[num] = new Vector2(6f, 2f);
		num++;
		_leftSideConnection[num] = -1;
		_rightSideConnection[num] = 0;
		_tileHeight[num][5] = 6;
		_tileHeight[num][6] = 7;
		_tileHeight[num][7] = 8;
		for (int num15 = 0; num15 < 5; num15++)
		{
			_tileHeight[num][num15] = -2;
		}
		_texturePosition[num] = new Vector2(7f, 2f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = 1;
		_trackType[num] = 2;
		_boostLeft[num] = false;
		_texturePosition[num] = new Vector2(2f, 3f);
		num++;
		_leftSideConnection[num] = 1;
		_rightSideConnection[num] = 1;
		_trackType[num] = 2;
		_boostLeft[num] = true;
		_texturePosition[num] = new Vector2(3f, 3f);
		num++;
		_leftSideConnection[num] = 0;
		_rightSideConnection[num] = 2;
		for (int num16 = 0; num16 < 8; num16++)
		{
			_tileHeight[num][num16] = 8 - num16;
		}
		_trackType[num] = 2;
		_boostLeft[num] = false;
		_texturePosition[num] = new Vector2(4f, 3f);
		num++;
		_leftSideConnection[num] = 2;
		_rightSideConnection[num] = 0;
		for (int num17 = 0; num17 < 8; num17++)
		{
			_tileHeight[num][num17] = num17 + 1;
		}
		_trackType[num] = 2;
		_boostLeft[num] = true;
		_texturePosition[num] = new Vector2(5f, 3f);
		num++;
		_leftSideConnection[num] = 0;
		_rightSideConnection[num] = 2;
		for (int num18 = 0; num18 < 8; num18++)
		{
			_tileHeight[num][num18] = 8 - num18;
		}
		_trackType[num] = 2;
		_boostLeft[num] = true;
		_texturePosition[num] = new Vector2(6f, 3f);
		num++;
		_leftSideConnection[num] = 2;
		_rightSideConnection[num] = 0;
		for (int num19 = 0; num19 < 8; num19++)
		{
			_tileHeight[num][num19] = num19 + 1;
		}
		_trackType[num] = 2;
		_boostLeft[num] = false;
		_texturePosition[num] = new Vector2(7f, 3f);
		num++;
		_texturePosition[36] = new Vector2(0f, 6f);
		_texturePosition[37] = new Vector2(1f, 6f);
		_texturePosition[39] = new Vector2(0f, 7f);
		_texturePosition[38] = new Vector2(1f, 7f);
		for (int num20 = 0; num20 < _texturePosition.Length; num20++)
		{
			_texturePosition[num20] *= 18f;
		}
		for (int num21 = 0; num21 < _tileHeight.Length; num21++)
		{
			int[] array2 = _tileHeight[num21];
			for (int num22 = 0; num22 < array2.Length; num22++)
			{
				if (array2[num22] >= 0)
				{
					array2[num22] = (8 - array2[num22]) * 2;
				}
			}
		}
		int[] array3 = new int[36];
		_trackSwitchOptions = new int[64][];
		for (int num23 = 0; num23 < 64; num23++)
		{
			int num24 = 0;
			for (int num25 = 1; num25 < 256; num25 <<= 1)
			{
				if ((num23 & num25) == num25)
				{
					num24++;
				}
			}
			int num26 = 0;
			for (int num27 = 0; num27 < 36; num27++)
			{
				array3[num27] = -1;
				int num28 = 0;
				switch (_leftSideConnection[num27])
				{
				case 0:
					num28 |= 1;
					break;
				case 1:
					num28 |= 2;
					break;
				case 2:
					num28 |= 4;
					break;
				}
				switch (_rightSideConnection[num27])
				{
				case 0:
					num28 |= 8;
					break;
				case 1:
					num28 |= 0x10;
					break;
				case 2:
					num28 |= 0x20;
					break;
				}
				if (num24 < 2)
				{
					if (num23 != num28)
					{
						continue;
					}
				}
				else if (num28 == 0 || (num23 & num28) != num28)
				{
					continue;
				}
				array3[num27] = num27;
				num26++;
			}
			if (num26 == 0)
			{
				continue;
			}
			int[] array4 = new int[num26];
			int num29 = 0;
			for (int num30 = 0; num30 < 36; num30++)
			{
				if (array3[num30] != -1)
				{
					array4[num29] = array3[num30];
					num29++;
				}
			}
			_trackSwitchOptions[num23] = array4;
		}
		_firstPressureFrame = -1;
		_firstLeftBoostFrame = -1;
		_firstRightBoostFrame = -1;
		for (int num31 = 0; num31 < _trackType.Length; num31++)
		{
			switch (_trackType[num31])
			{
			case 1:
				if (_firstPressureFrame == -1)
				{
					_firstPressureFrame = (short)num31;
				}
				break;
			case 2:
				if (_boostLeft[num31])
				{
					if (_firstLeftBoostFrame == -1)
					{
						_firstLeftBoostFrame = (short)num31;
					}
				}
				else if (_firstRightBoostFrame == -1)
				{
					_firstRightBoostFrame = (short)num31;
				}
				break;
			}
		}
	
	}
	public static bool IsPressurePlate(Tile tile)
{
	
		if (tile == null)
		{
			return false;
		}
		if (tile.active() && tile.type == 314 && (tile.frameX == 20 || tile.frameX == 21))
		{
			return true;
		}
		return false;
	
	}
	public static bool FrameTrack(int i, int j, bool pound, bool mute = false)
{
	
		if (_trackType == null)
		{
			return false;
		}
		Tile tile = Main.tile[i, j];
		if (tile == null)
		{
			tile = new Tile();
			Main.tile[i, j] = tile;
		}
		if (mute && tile.type != 314)
		{
			return false;
		}
		int nearbyTilesSetLookupIndex = GetNearbyTilesSetLookupIndex(i, j);
		int num = tile.FrontTrack();
		int num2 = tile.BackTrack();
		int num3 = ((num >= 0 && num < _trackType.Length) ? _trackType[num] : 0);
		int num4 = -1;
		int num5 = -1;
		int[] array = _trackSwitchOptions[nearbyTilesSetLookupIndex];
		if (array == null)
		{
			if (pound)
			{
				return false;
			}
			tile.FrontTrack(0);
			tile.BackTrack(-1);
			return false;
		}
		if (!pound)
		{
			int num6 = -1;
			int num7 = -1;
			bool flag = false;
			for (int k = 0; k < array.Length; k++)
			{
				int num8 = array[k];
				if (num2 == array[k])
				{
					num5 = k;
				}
				if (_trackType[num8] != num3)
				{
					continue;
				}
				if (_leftSideConnection[num8] == -1 || _rightSideConnection[num8] == -1)
				{
					if (num == array[k])
					{
						num4 = k;
						flag = true;
					}
					if (num6 == -1)
					{
						num6 = k;
					}
				}
				else
				{
					if (num == array[k])
					{
						num4 = k;
						flag = false;
					}
					if (num7 == -1)
					{
						num7 = k;
					}
				}
			}
			if (num7 != -1)
			{
				if (num4 == -1 || flag)
				{
					num4 = num7;
				}
			}
			else
			{
				if (num4 == -1)
				{
					switch (num3)
					{
					case 2:
						return false;
					case 1:
						return false;
					}
					num4 = num6;
				}
				num5 = -1;
			}
		}
		else
		{
			for (int l = 0; l < array.Length; l++)
			{
				if (num == array[l])
				{
					num4 = l;
				}
				if (num2 == array[l])
				{
					num5 = l;
				}
			}
			int num9 = 0;
			int num10 = 0;
			for (int m = 0; m < array.Length; m++)
			{
				if (_trackType[array[m]] == num3)
				{
					if (_leftSideConnection[array[m]] == -1 || _rightSideConnection[array[m]] == -1)
					{
						num10++;
					}
					else
					{
						num9++;
					}
				}
			}
			if (num9 < 2 && num10 < 2)
			{
				return false;
			}
			bool flag2 = num9 == 0;
			bool flag3 = false;
			if (!flag2)
			{
				while (!flag3)
				{
					num5++;
					if (num5 >= array.Length)
					{
						num5 = -1;
						break;
					}
					if ((_leftSideConnection[array[num5]] != _leftSideConnection[array[num4]] || _rightSideConnection[array[num5]] != _rightSideConnection[array[num4]]) && _trackType[array[num5]] == num3 && _leftSideConnection[array[num5]] != -1 && _rightSideConnection[array[num5]] != -1)
					{
						flag3 = true;
					}
				}
			}
			if (!flag3)
			{
				do
				{
					num4++;
					if (num4 >= array.Length)
					{
						num4 = -1;
						do
						{
							num4++;
						}
						while (_trackType[array[num4]] != num3 || (_leftSideConnection[array[num4]] == -1 || _rightSideConnection[array[num4]] == -1) != flag2);
						break;
					}
				}
				while (_trackType[array[num4]] != num3 || (_leftSideConnection[array[num4]] == -1 || _rightSideConnection[array[num4]] == -1) != flag2);
			}
		}
		bool flag4 = false;
		switch (num4)
		{
		case -2:
			if (tile.FrontTrack() != _firstPressureFrame)
			{
				flag4 = true;
			}
			break;
		case -1:
			if (tile.FrontTrack() != 0)
			{
				flag4 = true;
			}
			break;
		default:
			if (tile.FrontTrack() != array[num4])
			{
				flag4 = true;
			}
			break;
		}
		if (num5 == -1)
		{
			if (tile.BackTrack() != -1)
			{
				flag4 = true;
			}
		}
		else if (tile.BackTrack() != array[num5])
		{
			flag4 = true;
		}
		switch (num4)
		{
		case -2:
			tile.FrontTrack(_firstPressureFrame);
			break;
		case -1:
			tile.FrontTrack(0);
			break;
		default:
			tile.FrontTrack((short)array[num4]);
			break;
		}
		if (num5 == -1)
		{
			tile.BackTrack(-1);
		}
		else
		{
			tile.BackTrack((short)array[num5]);
		}
		if (pound && flag4 && !mute)
		{
			WorldGen.KillTile(i, j, fail: true);
		}
		return true;
	
	}
	private static int GetNearbyTilesSetLookupIndex(int i, int j)
{
	
		int num = 0;
		if (Main.tile[i - 1, j - 1] != null && Main.tile[i - 1, j - 1].type == 314)
		{
			num++;
		}
		if (Main.tile[i - 1, j] != null && Main.tile[i - 1, j].type == 314)
		{
			num += 2;
		}
		if (Main.tile[i - 1, j + 1] != null && Main.tile[i - 1, j + 1].type == 314)
		{
			num += 4;
		}
		if (Main.tile[i + 1, j - 1] != null && Main.tile[i + 1, j - 1].type == 314)
		{
			num += 8;
		}
		if (Main.tile[i + 1, j] != null && Main.tile[i + 1, j].type == 314)
		{
			num += 16;
		}
		if (Main.tile[i + 1, j + 1] != null && Main.tile[i + 1, j + 1].type == 314)
		{
			num += 32;
		}
		return num;
	
	}
	public static void PlaceTrack(Tile trackCache, int style)
{
	
		trackCache.active(active: true);
		trackCache.type = 314;
		trackCache.frameY = -1;
		switch (style)
		{
		case 0:
			trackCache.frameX = -1;
			break;
		case 1:
			trackCache.frameX = _firstPressureFrame;
			break;
		case 2:
			trackCache.frameX = _firstLeftBoostFrame;
			break;
		case 3:
			trackCache.frameX = _firstRightBoostFrame;
			break;
		}
	
	}
	public static int GetTrackItem(Tile trackCache)
{
	
		return _trackType[trackCache.frameX] switch
		{
			0 => 2340, 
			1 => 2492, 
			2 => 2739, 
			_ => 0, 
		};
	
	}

	private static short FrontTrack(this Tile tileTrack)
{
	
		return tileTrack.frameX;
	
	}
	private static void FrontTrack(this Tile tileTrack, short trackID)
{
	
		tileTrack.frameX = trackID;
	
	}
	private static short BackTrack(this Tile tileTrack)
{
	
		return tileTrack.frameY;
	
	}
	private static void BackTrack(this Tile tileTrack, short trackID)
{
	
		tileTrack.frameY = trackID;
	
	}
}
