namespace Terraria.Player;

public static class PlayerSetMatchQuery
{
  public static PlayerSetMatchResult Evaluate(in PlayerSetMatchInput input)
  {
    int requestedSlot = input.ArmorSlotRequested;
    int selectedSlot = requestedSlot switch
    {
      1 => input.Body,
      2 => input.Legs,
      _ => input.Head,
    };

    int matchedSlot = -1;
    bool somethingSpecial = input.SomethingSpecial;

    if (requestedSlot == 0 && selectedSlot == 201)
    {
      matchedSlot = !input.MountActive || input.MountType != 54
        ? input.Male ? 201 : 202
        : 201;
    }

    if (requestedSlot == 1)
    {
      bool matchedBodyIsSpecial = true;
      switch (selectedSlot)
      {
        case 15:
          matchedSlot = 88;
          break;
        case 36:
          matchedSlot = 89;
          break;
        case 41:
          matchedSlot = 97;
          break;
        case 42:
          matchedSlot = 90;
          break;
        case 58:
          matchedSlot = 91;
          break;
        case 59:
          matchedSlot = 92;
          break;
        case 60:
          matchedSlot = 93;
          break;
        case 61:
          matchedSlot = 94;
          break;
        case 62:
          matchedSlot = 95;
          break;
        case 63:
          matchedSlot = 96;
          break;
        case 77:
          matchedSlot = 121;
          break;
        case 165:
          matchedSlot = input.Male ? 118 : 99;
          break;
        case 166:
          matchedBodyIsSpecial = false;
          matchedSlot = input.Male ? 119 : 100;
          break;
        case 167:
          matchedSlot = input.Male ? 101 : 102;
          break;
        case 180:
          matchedSlot = 115;
          break;
        case 181:
          matchedSlot = 116;
          break;
        case 183:
          matchedSlot = input.Male ? 136 : 123;
          break;
        case 191:
          matchedSlot = 131;
          break;
        case 93:
          matchedSlot = 165;
          break;
        case 90:
          matchedSlot = 166;
          break;
        case 88:
          matchedSlot = 168;
          break;
        case 81:
          if (input.Legs == -1 || input.Legs == 0)
          {
            matchedSlot = 169;
          }
          break;
        case 213:
          matchedSlot = 187;
          break;
        case 215:
          matchedSlot = 189;
          break;
        case 219:
          matchedSlot = 196;
          break;
        case 221:
          matchedSlot = 199;
          break;
        case 223:
          matchedSlot = 204;
          break;
        case 231:
          matchedSlot = 214;
          break;
        case 232:
          matchedSlot = 215;
          break;
        case 233:
          matchedSlot = 216;
          break;
        case 241:
          matchedSlot = 229;
          break;
        case 256:
          matchedSlot = 244;
          break;
      }

      if (matchedSlot != -1)
      {
        somethingSpecial = matchedBodyIsSpecial;
      }
    }

    if (requestedSlot == 2)
    {
      switch (selectedSlot)
      {
        case 83:
          if (input.Male)
          {
            matchedSlot = 117;
          }
          break;
        case 84:
          if (input.Male)
          {
            matchedSlot = 120;
          }
          break;
        case 132:
          if (input.Male)
          {
            matchedSlot = 135;
          }
          break;
        case 57:
          if (input.Male)
          {
            matchedSlot = 137;
          }
          break;
        case 180:
          if (!input.Male)
          {
            matchedSlot = 179;
          }
          break;
        case 184:
          if (!input.Male)
          {
            matchedSlot = 183;
          }
          break;
        case 146:
          matchedSlot = input.Male ? 146 : 147;
          break;
        case 154:
          matchedSlot = input.Male ? 155 : 154;
          break;
        case 158:
          if (input.Male)
          {
            matchedSlot = 157;
          }
          break;
        case 191:
          if (!input.Male)
          {
            matchedSlot = 192;
          }
          break;
        case 193:
          if (!input.Male)
          {
            matchedSlot = 194;
          }
          break;
        case 197:
          if (!input.Male)
          {
            matchedSlot = 198;
          }
          break;
        case 203:
          if (!input.Male)
          {
            matchedSlot = 202;
          }
          break;
        case 208:
          if (!input.Male)
          {
            matchedSlot = 207;
          }
          break;
        case 219:
          if (!input.Male)
          {
            matchedSlot = 220;
          }
          break;
        case 232:
          if (!input.Male)
          {
            matchedSlot = 233;
          }
          break;
        case 236:
          if (!input.Male)
          {
            matchedSlot = 248;
          }
          break;
        case 249:
          if (!input.Male)
          {
            matchedSlot = 250;
          }
          break;
      }
    }

    return new PlayerSetMatchResult(
      MatchedSlot: matchedSlot,
      Matched: matchedSlot != -1,
      SomethingSpecial: somethingSpecial);
  }
}
