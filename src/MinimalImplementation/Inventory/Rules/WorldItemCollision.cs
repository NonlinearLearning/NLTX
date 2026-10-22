using System.Numerics;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Inventory.Rules;

/// <summary>
/// Item-specific Terraria319 collision over a detached World capture. Preserves the original
/// half-block, slope, hoik and conveyor order without static flags or live tile initialization.
/// </summary>
internal sealed class WorldItemCollision {
  private enum TileContactSide {
    Left, Right, Top, Bottom, BottomLeft, BottomRight
  }

  private readonly record struct TileContact(
    TileContactSide Side, int X, int Y, ushort Type, byte Slope, int Overlap);

  private readonly WorldSpatialSnapshot _world;

  internal WorldItemCollision(WorldSpatialSnapshot world) {
    _world = world;
  }

  internal Vector2 TileCollision(
      Vector2 position, Vector2 velocity, int width, int height,
      bool fallThrough = false, bool fall2 = false, int gravDir = 1,
      bool ignoreAetheriumPlatforms = false, bool hoik = true) {

    Vector2 result = velocity;
    Vector2 vector = velocity;
    Vector2 vector2 = position + velocity;
    Vector2 vector3 = position;
    int value = (int)(position.X / 16f) - 1;
    int value2 = (int)((position.X + (float)width) / 16f) + 2;
    int value3 = (int)(position.Y / 16f) - 1;
    int value4 = (int)((position.Y + (float)height) / 16f) + 2;
    int num = -1;
    int num2 = -1;
    int num3 = -1;
    int num4 = -1;
    int num5 = Math.Clamp(value, 0, _world.Header.Geometry.Width - 1);
    value2 = Math.Clamp(value2, 0, _world.Header.Geometry.Width - 1);
    value3 = Math.Clamp(value3, 0, _world.Header.Geometry.Height - 40);
    value4 = Math.Clamp(value4, 0, _world.Header.Geometry.Height - 40);
    float num6 = (value4 + 3) * 16;
    Vector2 vector4 = default(Vector2);
    for (int i = num5; i < value2; i++) {
      for (int j = value3; j < value4; j++) {
        WorldTile tile = GetTile(i, j);
        if (!tile.Active || tile.Inactive) {
          continue;
        }
        bool flag = _world.Facts.Solid[tile.Type];
        if (_world.Facts.SolidTop[tile.Type] && tile.FrameY == 0) {
          flag = true;
        }
        if (ignoreAetheriumPlatforms && tile.Type == 19 && tile.FrameY / 18 == 50) {
          flag = false;
        }
        if (!flag) {
          continue;
        }
        vector4.X = i * 16;
        vector4.Y = j * 16;
        int num7 = 16;
        if (GetTile(i, j).HalfBlock) {
          vector4.Y += 8f;
          num7 -= 8;
        }
        if (!(vector2.X + (float)width > vector4.X) || !(vector2.X < vector4.X + 16f) ||
            !(vector2.Y + (float)height > vector4.Y) || !(vector2.Y < vector4.Y + (float)num7)) {
          continue;
        }
        bool flag2 = false;
        bool flag3 = false;
        if (GetTile(i, j).Slope > 2) {
          if (GetTile(i, j).Slope == 3 && vector3.Y + Math.Abs(velocity.X) >= vector4.Y &&
              vector3.X >= vector4.X) {
            flag3 = true;
          }
          if (GetTile(i, j).Slope == 4 && vector3.Y + Math.Abs(velocity.X) >= vector4.Y &&
              vector3.X + (float)width <= vector4.X + 16f) {
            flag3 = true;
          }
        } else if (GetTile(i, j).Slope > 0) {
          flag2 = true;
          if (GetTile(i, j).Slope == 1 &&
              vector3.Y + (float)height - Math.Abs(velocity.X) <= vector4.Y + (float)num7 &&
              vector3.X >= vector4.X) {
            flag3 = true;
          }
          if (GetTile(i, j).Slope == 2 &&
              vector3.Y + (float)height - Math.Abs(velocity.X) <= vector4.Y + (float)num7 &&
              vector3.X + (float)width <= vector4.X + 16f) {
            flag3 = true;
          }
        }
        if (flag3) {
          continue;
        }
        if (vector3.Y + (float)height <= vector4.Y) {
          if ((!(_world.Facts.SolidTop[GetTile(i, j).Type] && fallThrough) || !(velocity.Y <= 1f ||
              fall2)) && num6 > vector4.Y) {
            num3 = i;
            num4 = j;
            if (num7 < 16) {
              num4++;
            }
            if (num3 != num && !flag2) {
              result.Y = vector4.Y - (vector3.Y + (float)height) +
                  ((gravDir == -1) ? (-0.01f) : 0f);
              num6 = vector4.Y;
            }
          }
        } else if (vector3.X + (float)width <= vector4.X && !_world.Facts.SolidTop[GetTile(i,
            j).Type]) {
          if (!hoik || i < 1 || (GetTile(i - 1, j).Slope != 2 && GetTile(i - 1, j).Slope != 4)) {
            num = i;
            num2 = j;
            if (num2 != num4) {
              result.X = vector4.X - (vector3.X + (float)width);
            }
            if (num3 == num) {
              result.Y = vector.Y;
            }
          }
        } else if (vector3.X >= vector4.X + 16f && !_world.Facts.SolidTop[GetTile(i, j).Type]) {
          if (!hoik || (GetTile(i + 1, j).Slope != 1 && GetTile(i + 1, j).Slope != 3)) {
            num = i;
            num2 = j;
            if (num2 != num4) {
              result.X = vector4.X + 16f - vector3.X;
            }
            if (num3 == num) {
              result.Y = vector.Y;
            }
          }
        } else if (vector3.Y >= vector4.Y + (float)num7 && !_world.Facts.SolidTop[GetTile(i,
            j).Type]) {
          num3 = i;
          num4 = j;
          result.Y = vector4.Y + (float)num7 - vector3.Y + ((gravDir == 1) ? 0.01f : 0f);
          if (num4 == num2) {
            result.X = vector.X;
          }
        }
      }
    }
    return result;
  }

  internal Vector4 SlopeCollision(
      Vector2 position, Vector2 velocity, int width, int height,
      bool fall = false, bool ignoreAetheriumPlatforms = false) {

    var slopes = new bool[5];
    float y = position.Y;
    float y2 = position.Y;
    Vector2 vector = position;
    Vector2 vector2 = position;
    Vector2 vector3 = velocity;
    int value = (int)(position.X / 16f) - 1;
    int value2 = (int)((position.X + (float)width) / 16f) + 2;
    int value3 = (int)(position.Y / 16f) - 1;
    int value4 = (int)((position.Y + (float)height) / 16f) + 2;
    int num = Math.Clamp(value, 0, _world.Header.Geometry.Width - 1);
    value2 = Math.Clamp(value2, 0, _world.Header.Geometry.Width - 1);
    value3 = Math.Clamp(value3, 0, _world.Header.Geometry.Height - 40);
    value4 = Math.Clamp(value4, 0, _world.Header.Geometry.Height - 40);
    Vector2 vector4 = default(Vector2);
    for (int i = num; i < value2; i++) {
      for (int j = value3; j < value4; j++) {
        WorldTile tile = GetTile(i, j);
        if (!tile.Active || tile.Inactive) {
          continue;
        }
        bool flag = _world.Facts.Solid[tile.Type];
        if (_world.Facts.SolidTop[tile.Type] && tile.FrameY == 0) {
          flag = true;
        }
        if (ignoreAetheriumPlatforms && tile.Type == 19 && tile.FrameY / 18 == 50) {
          flag = false;
        }
        if (!flag) {
          continue;
        }
        vector4.X = i * 16;
        vector4.Y = j * 16;
        int num2 = 16;
        if (GetTile(i, j).HalfBlock) {
          vector4.Y += 8f;
          num2 -= 8;
        }
        if (!(position.X + (float)width > vector4.X) || !(position.X < vector4.X + 16f) ||
            !(position.Y + (float)height > vector4.Y) || !(position.Y < vector4.Y + (float)num2)) {
          continue;
        }
        bool flag2 = true;
        if (IsPlatform(GetTile(i, j).Type)) {
          if (velocity.Y < 0f) {
            flag2 = false;
          }
          if (position.Y + (float)height < (float)(j * 16) ||
              position.Y + (float)height - (1f + Math.Abs(velocity.X)) > (float)(j * 16 + 16)) {
            flag2 = false;
          }
          if (((GetTile(i, j).Slope == 1 && velocity.X >= 0f) || (GetTile(i, j).Slope == 2 &&
              velocity.X <= 0f)) && (position.Y + (float)height) / 16f - 1f == (float)j) {
            flag2 = false;
          }
        }
        if (!flag2) {
          continue;
        }
        bool flag3 = false;
        if (fall && IsPlatform(GetTile(i, j).Type)) {
          flag3 = true;
        }
        int num3 = GetTile(i, j).Slope;
        vector4.X = i * 16;
        vector4.Y = j * 16;
        if (!(position.X + (float)width > vector4.X) || !(position.X < vector4.X + 16f) ||
            !(position.Y + (float)height > vector4.Y) || !(position.Y < vector4.Y + 16f)) {
          continue;
        }
        float num4 = 0f;
        if (num3 == 3 || num3 == 4) {
          if (num3 == 3) {
            num4 = position.X - vector4.X;
          }
          if (num3 == 4) {
            num4 = vector4.X + 16f - (position.X + (float)width);
          }
          if (num4 >= 0f) {
            if (position.Y <= vector4.Y + 16f - num4) {
              float num5 = vector4.Y + 16f - vector.Y - num4;
              if (position.Y + num5 > y2) {
                vector2.Y = position.Y + num5;
                y2 = vector2.Y;
                if (vector3.Y < 0.0101f) {
                  vector3.Y = 0.0101f;
                }
                slopes[num3] = true;
              }
            }
          } else if (position.Y > vector4.Y) {
            float num6 = vector4.Y + 16f;
            if (vector2.Y < num6) {
              vector2.Y = num6;
              if (vector3.Y < 0.0101f) {
                vector3.Y = 0.0101f;
              }
            }
          }
        }
        if (num3 != 1 && num3 != 2) {
          continue;
        }
        if (num3 == 1) {
          num4 = position.X - vector4.X;
        }
        if (num3 == 2) {
          num4 = vector4.X + 16f - (position.X + (float)width);
        }
        if (num4 >= 0f) {
          if (!(position.Y + (float)height >= vector4.Y + num4)) {
            continue;
          }
          float num7 = vector4.Y - (vector.Y + (float)height) + num4;
          if (!(position.Y + num7 < y)) {
            continue;
          }
          if (flag3) {
            continue;
          }
          vector2.Y = position.Y + num7;
          y = vector2.Y;
          if (vector3.Y > 0f) {
            vector3.Y = 0f;
          }
          slopes[num3] = true;
          continue;
        }
        if (IsPlatform(GetTile(i, j).Type) &&
            !(position.Y + (float)height - 4f - Math.Abs(velocity.X) <= vector4.Y)) {
          continue;
        }
        float num8 = vector4.Y - (float)height;
        if (!(vector2.Y > num8)) {
          continue;
        }
        if (flag3) {
          continue;
        }
        vector2.Y = num8;
        if (vector3.Y > 0f) {
          vector3.Y = 0f;
        }
      }
    }
    Vector2 correction = vector2 - position;
    Vector2 vector5 = TileCollision(position, correction, width, height);
    if (vector5.Y > correction.Y) {
      float num9 = correction.Y - vector5.Y;
      vector2.Y = position.Y + vector5.Y;
      if (slopes[1]) {
        vector2.X = position.X - num9;
      }
      if (slopes[2]) {
        vector2.X = position.X + num9;
      }
      vector3.X = 0f;
      vector3.Y = 0f;
    } else if (vector5.Y < correction.Y) {
      float num10 = vector5.Y - correction.Y;
      vector2.Y = position.Y + vector5.Y;
      if (slopes[3]) {
        vector2.X = position.X - num10;
      }
      if (slopes[4]) {
        vector2.X = position.X + num10;
      }
      vector3.X = 0f;
      vector3.Y = 0f;
    }
    return new Vector4(vector2, vector3.X, vector3.Y);
  }

  private void BuildTileContacts(
      Vector2 position, int width, int height, List<TileContact> contactTiles) {
    position.X = (int)position.X;
    position.Y = (int)position.Y;
    contactTiles.Clear();
    int value = (int)((position.X - 1f) / 16f) - 1;
    int value2 = (int)((position.X + 1f + (float)width) / 16f) + 1;
    int value3 = (int)((position.Y - 1f) / 16f) - 1;
    int value4 = (int)((position.Y + 3f + (float)height) / 16f) + 1;
    int num = Math.Clamp(value, 0, _world.Header.Geometry.Width - 1);
    value2 = Math.Clamp(value2, 0, _world.Header.Geometry.Width - 1);
    value3 = Math.Clamp(value3, 0, _world.Header.Geometry.Height - 40);
    value4 = Math.Clamp(value4, 0, _world.Header.Geometry.Height - 40);
    Vector2 vector = default(Vector2);
    for (int i = num; i < value2; i++) {
      for (int j = value3; j < value4; j++) {
        WorldTile tile = GetTile(i, j);
        if (!tile.Active || tile.Inactive) {
          continue;
        }
        ushort type = tile.Type;
        bool flag = _world.Facts.Solid[type];
        if (_world.Facts.SolidTop[type] && tile.FrameY == 0) {
          flag = true;
        }
        if (!flag) {
          continue;
        }
        vector.X = i * 16;
        vector.Y = j * 16;
        int num2 = 16;
        if (tile.HalfBlock) {
          vector.Y += 8f;
          num2 -= 8;
        }
        byte b = tile.Slope;
        if (Math.Abs(position.X - (vector.X + 16f)) < 0.1f &&
            position.Y + (float)height > vector.Y && position.Y < vector.Y + (float)num2 &&
            b != 3 && b != 1) {
          float num3 = Math.Max(Math.Min(position.Y + (float)height,
              vector.Y + (float)num2) - Math.Max(position.Y, vector.Y) + 0.5f, 1f);
          contactTiles.Add(new TileContact(TileContactSide.Left, i, j, type, b, (int)num3));
        }
        if (Math.Abs(position.X + (float)width - vector.X) < 0.1f &&
            position.Y + (float)height > vector.Y && position.Y < vector.Y + (float)num2 &&
            b != 4 && b != 2) {
          float num4 = Math.Max(Math.Min(position.Y + (float)height,
              vector.Y + (float)num2) - Math.Max(position.Y, vector.Y) + 0.5f, 1f);
          contactTiles.Add(new TileContact(TileContactSide.Right, i, j, type, b, (int)num4));
        }
        if (!(position.Y + 3f + (float)height > vector.Y) ||
            !(position.Y - 1f < vector.Y + (float)num2)) {
          continue;
        }
        if (position.X + (float)width > vector.X && position.X < vector.X + 16f) {
          float num5 = Math.Max(Math.Min(position.X + (float)width,
              vector.X + 16f) - Math.Max(position.X, vector.X) + 0.5f, 1f);
          switch (b) {
          case 0:
            if (Math.Abs(position.Y - (vector.Y + (float)num2)) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Top, i, j, type, b, (int)num5));
            }
            if (Math.Abs(position.Y + (float)height - vector.Y) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Bottom, i, j, type, b, (int)num5));
            }
            break;
          case 1: {
            if (Math.Abs(position.Y - (vector.Y + (float)num2)) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Top, i, j, type, b, (int)num5));
            }
            float num9 = Math.Max(position.X - vector.X, 0f);
            float num10 = position.Y + (float)height;
            if (num10 - vector.Y > -0.1f && num10 - (vector.Y + num9) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Bottom, i, j, type, b, (int)num5));
            }
            break;
          }
          case 2: {
            if (Math.Abs(position.Y - (vector.Y + (float)num2)) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Top, i, j, type, b, (int)num5));
            }
            float num7 = Math.Max(vector.X + 16f - (position.X + (float)width), 0f);
            float num8 = position.Y + (float)height;
            if (num8 - vector.Y > -0.1f && num8 - (vector.Y + num7) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Bottom, i, j, type, b, (int)num5));
            }
            break;
          }
          case 3: {
            float num11 = Math.Max(position.X - vector.X, 0f);
            if (Math.Abs(position.Y - (vector.Y + (float)num2 - num11)) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Top, i, j, type, b, (int)num5));
            }
            if (Math.Abs(position.Y + (float)height - vector.Y) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Bottom, i, j, type, b, (int)num5));
            }
            break;
          }
          case 4: {
            float num6 = Math.Max(vector.X + 16f - (position.X + (float)width), 0f);
            if (Math.Abs(position.Y - (vector.Y + (float)num2 - num6)) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Top, i, j, type, b, (int)num5));
            }
            if (Math.Abs(position.Y + (float)height - vector.Y) < 0.1f) {
              contactTiles.Add(new TileContact(TileContactSide.Bottom, i, j, type, b, (int)num5));
            }
            break;
          }
          }
        } else {
          if (!(position.X + 3f + (float)width > vector.X) ||
              !(position.X - 3f < vector.X + 16f) || !(position.Y < vector.Y)) {
            continue;
          }
          TileContactSide tileContactSide = vector.X < position.X
              ? TileContactSide.BottomLeft : TileContactSide.BottomRight;
          switch (b) {
          case 0:
          case 3:
          case 4:
            contactTiles.Add(new TileContact(tileContactSide, i, j, type, b, 0));
            break;
          case 1:
            if (tileContactSide == TileContactSide.BottomRight) {
              contactTiles.Add(new TileContact(tileContactSide, i, j, type, b, 0));
            }
            break;
          case 2:
            if (tileContactSide == TileContactSide.BottomLeft) {
              contactTiles.Add(new TileContact(tileContactSide, i, j, type, b, 0));
            }
            break;
          }
        }
      }
    }
  }

  internal bool ApplyConveyor(
      ref Vector2 itemPosition, ref Vector2 velocity, int width, int height) {
    var contacts = new List<TileContact>();
    bool flag = false;
    BuildTileContacts(itemPosition, width, height, contacts);
    if (contacts.Count > 0) {
      int num = -1;
      int num2 = -1;
      bool flag2 = false;
      TileContactSide tileContactSide = TileContactSide.Top;
      TileContactSide tileContactSide2 = TileContactSide.Top;
      Vector2 zero = Vector2.Zero;
      bool flag3 = false;
      bool flag4 = false;
      bool flag5 = false;
      for (int i = 0; i < contacts.Count; i++) {
        int num3 = ConveyorDirection(contacts[i].Type);
        switch (contacts[i].Side) {
        case TileContactSide.Left:
          zero.Y += num3 * contacts[i].Overlap;
          flag3 = flag3 || num3 != 0;
          break;
        case TileContactSide.Right:
          zero.Y += -num3 * contacts[i].Overlap;
          flag3 = flag3 || num3 != 0;
          break;
        case TileContactSide.Top:
          zero.X += -num3 * contacts[i].Overlap;
          flag4 = num3 != 0;
          break;
        case TileContactSide.Bottom:
          flag5 = true;
          zero.X += num3 * contacts[i].Overlap;
          flag4 = num3 != 0;
          if (contacts[i].Slope == 1) {
            if ((float)(contacts[i].X * 16) < itemPosition.X) {
              WorldTile tile3 = GetTile(contacts[i].X, contacts[i].Y - 1);
              if (tile3.Active && tile3.Slope == 3) {
                num2 = i;
              }
            }
          } else if (contacts[i].Slope == 2 &&
              (float)(contacts[i].X * 16 + 16) > (itemPosition.X + width)) {
            WorldTile tile4 = GetTile(contacts[i].X, contacts[i].Y - 1);
            if (tile4.Active && tile4.Slope == 4) {
              num = i;
            }
          }
          break;
        case TileContactSide.BottomLeft:
          if (num3 == -1) {
            int x2 = contacts[i].X;
            int y2 = contacts[i].Y;
            WorldTile tile2 = GetTile(x2, y2 - 1);
            byte b2 = tile2.Slope;
            if (!tile2.Active || (b2 == 1 && ConveyorDirection(tile2.Type) == -1)) {
              tileContactSide = TileContactSide.BottomLeft;
              flag2 = GetTile(x2, y2).HalfBlock;
            }
          }
          if (num3 == 1) {
            tileContactSide2 = TileContactSide.BottomLeft;
          }
          break;
        case TileContactSide.BottomRight:
          if (num3 == 1) {
            int x = contacts[i].X;
            int y = contacts[i].Y;
            WorldTile tile = GetTile(x, y - 1);
            byte b = tile.Slope;
            if (!tile.Active || (b == 2 && ConveyorDirection(tile.Type) == 1)) {
              tileContactSide = TileContactSide.BottomRight;
              flag2 = GetTile(x, y).HalfBlock;
            }
          }
          if (num3 == -1) {
            tileContactSide2 = TileContactSide.BottomRight;
          }
          break;
        }
      }
      if (zero.X < 0f) {
        if (zero.X < -8f) {
          zero.X = -2.5f;
        } else if (zero.X < -4f) {
          zero.X = -1.25f;
        } else {
          zero.X = -0.75f;
        }
      } else if (zero.X > 0f) {
        if (zero.X > 8f) {
          zero.X = 2.5f;
        } else if (zero.X > 4f) {
          zero.X = 1.25f;
        } else {
          zero.X = 0.75f;
        }
      }
      if (zero.Y < 0f) {
        if (zero.Y < 8f) {
          zero.Y = -2.5f;
        } else {
          zero.Y = -1.25f;
        }
      } else if (zero.Y > 0f) {
        if (zero.Y > 8f) {
          zero.Y = 2.5f;
        } else {
          zero.Y = 1.25f;
        }
      } else if (flag3 && velocity.Y <= 1f) {
        velocity.Y = 0f;
      }
      if (zero.Y == 0f) {
        if (velocity.Y < 0.11f && tileContactSide != TileContactSide.Top && !flag5) {
          if (tileContactSide == TileContactSide.BottomLeft) {
            flag = true;
            Vector2 bottom = new Vector2(itemPosition.X + width * 0.5f, itemPosition.Y + height);
            int num4 = (int)bottom.Y;
            num4 = (num4 + 8) / 16;
            num4 *= 16;
            if (flag2) {
              num4 += 8;
            }
            bottom.Y = num4;
            itemPosition = bottom - new Vector2(width * 0.5f, height);
            velocity.Y = 0f;
            velocity.X = -1.25f;
            zero.X = -2.5f;
          }
          if (tileContactSide == TileContactSide.BottomRight) {
            flag = true;
            Vector2 bottom2 = new Vector2(itemPosition.X + width * 0.5f, itemPosition.Y + height);
            int num5 = (int)bottom2.Y;
            num5 = (num5 + 8) / 16;
            num5 *= 16;
            if (flag2) {
              num5 += 8;
            }
            bottom2.Y = num5;
            itemPosition = bottom2 - new Vector2(width * 0.5f, height);
            velocity.Y = 0f;
            velocity.X = 1.25f;
            zero.X = 2.5f;
          }
        } else if ((double)velocity.Y < 0.3 && !flag5) {
          switch (tileContactSide2) {
          case TileContactSide.BottomRight:
            if (velocity.X <= 0.75f && velocity.X >= -2.5f && !flag3) {
              flag = true;
              int num7 = (int)itemPosition.X;
              num7 = (num7 + 8) / 16;
              num7 *= 16;
              itemPosition.X = num7;
              velocity.X = 0f;
              velocity.Y = 0.75f;
              zero.Y = 2.5f;
            }
            break;
          case TileContactSide.BottomLeft:
            if (velocity.X >= -0.75f && velocity.X <= 2.5f && !flag3) {
              flag = true;
              Vector2 right = new Vector2(itemPosition.X + width, itemPosition.Y + height * 0.5f);
              int num6 = (int)right.X;
              num6 = (num6 + 8) / 16;
              num6 *= 16;
              right.X = num6;
              itemPosition = right - new Vector2(width, height * 0.5f);
              velocity.X = 0f;
              velocity.Y = 0.75f;
              zero.Y = 2.5f;
            }
            break;
          }
        }
      }
      flag = flag || flag4 || flag3;
      if (zero.Y < 0f) {
        velocity.Y = Math.Max(velocity.Y + zero.Y * 6f / 60f, zero.Y);
      } else if (zero.Y > 0f) {
        velocity.Y = Math.Min(velocity.Y + zero.Y * 6f / 60f, zero.Y);
      }
      if (zero.X < 0f && velocity.X > zero.X) {
        velocity.X = Math.Max(velocity.X + zero.X * 6f / 60f, zero.X);
      }
      if (zero.X > 0f && velocity.X < zero.X) {
        velocity.X = Math.Min(velocity.X + zero.X * 6f / 60f, zero.X);
      }
      if (num != -1 && velocity.X > 0f) {
        int num8 = contacts[num].X * 16 + Math.Max(16 - height / 2, 0);
        Vector2 right2 = new Vector2(itemPosition.X + width, itemPosition.Y + height * 0.5f);
        if (right2.X + velocity.X > (float)num8) {
          if (right2.X >= (float)num8) {
            right2.X = num8;
            velocity.X = 0f;
            itemPosition = right2 - new Vector2(width, height * 0.5f);
          } else {
            velocity.X = (float)num8 - right2.X;
          }
        }
      }
      if (num2 != -1 && velocity.X < 0f) {
        int num9 = contacts[num2].X * 16 + Math.Min(height / 2, 16);
        Vector2 position = itemPosition;
        if (position.X + velocity.X < (float)num9) {
          if (position.X <= (float)num9) {
            position.X = num9;
            velocity.X = 0f;
            itemPosition = position;
          } else {
            velocity.X = (float)num9 - position.X;
          }
        }
      }
    }
    return flag;
  }

  private WorldTile GetTile(int x, int y) {
    var coordinate = new TileCoordinate(x, y);
    return _world.Header.Geometry.Contains(coordinate)
      ? _world.GetTile(coordinate) : WorldTile.Empty;
  }

  private static bool IsPlatform(ushort type) {
    return type is 19 or 427 or 435 or 436 or 437 or 438 or 439;
  }

  private static int ConveyorDirection(ushort type) {
    return type == 421 ? 1 : type == 422 ? -1 : 0;
  }
}
