using System;
using NSSLC.WorldGeneration.ID;
using NSSLC.WorldGeneration.Utilities;
namespace NSSLC.WorldGeneration;
public sealed partial class Item {
	public bool CanHavePrefixes()
{
	
		return GetRollablePrefixes() != null;
	
	}
	public bool Prefix(int prefixWeWant)
{
	
		bool rolledPrefixIsTopTier;
		return Prefix(prefixWeWant, out rolledPrefixIsTopTier);
	
	}
	public bool Prefix(int prefixWeWant, out bool rolledPrefixIsTopTier)
{
	
		if (!WorldGen.isGeneratingOrLoadingWorld && Main.rand == null)
		{
			Main.rand = new UnifiedRandom();
		}
		rolledPrefixIsTopTier = false;
		if (prefixWeWant == 0)
		{
			return false;
		}
		if (!CanHavePrefixes())
		{
			return false;
		}
		if (prefixWeWant == -3)
		{
			return true;
		}
		float num = 0f;
		if (prefixWeWant == -2 || prefixWeWant == -1)
		{
			num = BestPrefixValue();
		}
		UnifiedRandom unifiedRandom = (WorldGen.isGeneratingOrLoadingWorld ? WorldGen.genRand : Main.rand);
		int rolledPrefix = prefixWeWant;
		float dmg = 1f;
		float kb = 1f;
		float spd = 1f;
		float size = 1f;
		float shtspd = 1f;
		float mcst = 1f;
		int crt = 0;
		int tagdmg = 0;
		int arpen = 0;
		float num2 = 0f;
		bool flag = true;
		while (flag)
		{
			flag = false;
			if (rolledPrefix == -1 && unifiedRandom.Next(4) == 0)
			{
				rolledPrefix = 0;
			}
			if (prefixWeWant < -1)
			{
				rolledPrefix = -1;
			}
			if ((rolledPrefix == -1 || rolledPrefix == -2 || rolledPrefix == -3) && !RollAPrefix(unifiedRandom, ref rolledPrefix))
			{
				return false;
			}
			if (prefixWeWant == -1 && PrefixID.Sets.ReducedNaturalChance[rolledPrefix] && unifiedRandom.Next(3) != 0)
			{
				rolledPrefix = 0;
			}
			if (prefixWeWant == -4)
			{
				rolledPrefix = 0;
			}
			if (!TryGetPrefixStatMultipliersForItem(rolledPrefix, out dmg, out kb, out spd, out size, out shtspd, out mcst, out crt, out tagdmg, out arpen, out num2))
			{
				flag = true;
				rolledPrefix = -1;
			}
			if (prefixWeWant == -2 && rolledPrefix == 0)
			{
				rolledPrefix = -1;
				flag = true;
			}
		}
		rolledPrefixIsTopTier = num2 == num;
		damage = (int)Math.Round((float)damage * dmg);
		useAnimation = (int)Math.Round((float)useAnimation * spd);
		useTime = (int)Math.Round((float)useTime * spd);
		reuseDelay = (int)Math.Round((float)reuseDelay * spd);
		mana = (int)Math.Round((float)mana * mcst);
		knockBack *= kb;
		scale *= size;
		shootSpeed *= shtspd;
		crit += crt;
		bonusTagDamage += tagdmg;
		armorPenetration += arpen;
		if ((double)num2 >= 1.2)
		{
			rare += 2;
		}
		else if ((double)num2 >= 1.05)
		{
			rare++;
		}
		else if ((double)num2 <= 0.8)
		{
			rare -= 2;
		}
		else if ((double)num2 <= 0.95)
		{
			rare--;
		}
		if (rare > -11)
		{
			if (rare < -1)
			{
				rare = -1;
			}
			if (rare > 11)
			{
				rare = 11;
			}
		}
		num2 *= num2;
		value = (int)((float)value * num2);
		prefix = (byte)rolledPrefix;
		return true;
	
	}
	public bool TryGetPrefixStatMultipliersForItem(int rolledPrefix, out float dmg, out float kb, out float spd, out float size, out float shtspd, out float mcst, out int crt, out int tagdmg, out int arpen, out float value)
{
	
		dmg = 1f;
		kb = 1f;
		spd = 1f;
		size = 1f;
		shtspd = 1f;
		mcst = 1f;
		crt = 0;
		tagdmg = 0;
		arpen = 0;
		switch (rolledPrefix)
		{
		case 1:
			size = 1.12f;
			break;
		case 2:
			size = 1.18f;
			break;
		case 3:
			dmg = 1.05f;
			crt = 2;
			size = 1.05f;
			break;
		case 4:
			dmg = 1.1f;
			size = 1.1f;
			kb = 1.1f;
			break;
		case 5:
			dmg = 1.15f;
			break;
		case 6:
			dmg = 1.1f;
			break;
		case 81:
			kb = 1.15f;
			dmg = 1.15f;
			crt = 5;
			spd = 0.9f;
			size = 1.1f;
			break;
		case 7:
			size = 0.82f;
			break;
		case 8:
			kb = 0.85f;
			dmg = 0.85f;
			size = 0.87f;
			break;
		case 9:
			size = 0.9f;
			break;
		case 10:
			dmg = 0.85f;
			break;
		case 11:
			spd = 1.1f;
			kb = 0.9f;
			size = 0.9f;
			break;
		case 12:
			kb = 1.1f;
			dmg = 1.05f;
			size = 1.1f;
			spd = 1.15f;
			break;
		case 13:
			kb = 0.8f;
			dmg = 0.9f;
			size = 1.1f;
			break;
		case 14:
			kb = 1.15f;
			spd = 1.1f;
			break;
		case 15:
			kb = 0.9f;
			spd = 0.85f;
			break;
		case 16:
			dmg = 1.1f;
			crt = 3;
			break;
		case 17:
			spd = 0.85f;
			shtspd = 1.1f;
			break;
		case 18:
			spd = 0.9f;
			shtspd = 1.15f;
			break;
		case 19:
			kb = 1.15f;
			shtspd = 1.05f;
			break;
		case 20:
			kb = 1.05f;
			shtspd = 1.05f;
			dmg = 1.1f;
			spd = 0.95f;
			crt = 2;
			break;
		case 21:
			kb = 1.15f;
			dmg = 1.1f;
			break;
		case 82:
			kb = 1.15f;
			dmg = 1.15f;
			crt = 5;
			spd = 0.9f;
			shtspd = 1.1f;
			break;
		case 22:
			kb = 0.9f;
			shtspd = 0.9f;
			dmg = 0.85f;
			break;
		case 23:
			spd = 1.15f;
			shtspd = 0.9f;
			break;
		case 24:
			spd = 1.1f;
			kb = 0.8f;
			break;
		case 25:
			spd = 1.1f;
			dmg = 1.15f;
			crt = 1;
			break;
		case 58:
			spd = 0.85f;
			dmg = 0.85f;
			break;
		case 26:
			mcst = 0.85f;
			dmg = 1.1f;
			break;
		case 27:
			mcst = 0.85f;
			break;
		case 28:
			mcst = 0.85f;
			dmg = 1.15f;
			kb = 1.05f;
			break;
		case 83:
			kb = 1.15f;
			dmg = 1.15f;
			crt = 5;
			spd = 0.9f;
			mcst = 0.9f;
			break;
		case 29:
			mcst = 1.1f;
			break;
		case 30:
			mcst = 1.2f;
			dmg = 0.9f;
			break;
		case 31:
			kb = 0.9f;
			dmg = 0.9f;
			break;
		case 32:
			mcst = 1.15f;
			dmg = 1.1f;
			break;
		case 33:
			mcst = 1.1f;
			kb = 1.1f;
			spd = 0.9f;
			break;
		case 34:
			mcst = 0.9f;
			kb = 1.1f;
			spd = 1.1f;
			dmg = 1.1f;
			break;
		case 35:
			mcst = 1.2f;
			dmg = 1.15f;
			kb = 1.15f;
			break;
		case 52:
			mcst = 0.9f;
			dmg = 0.9f;
			spd = 0.9f;
			break;
		case 85:
			dmg = 1.15f;
			kb = 1.15f;
			arpen = 10;
			tagdmg = 3;
			break;
		case 86:
			dmg = 1.1f;
			kb = 1.05f;
			arpen = 5;
			tagdmg = 3;
			break;
		case 87:
			dmg = 1.15f;
			arpen = 8;
			break;
		case 88:
			dmg = 1.1f;
			tagdmg = 3;
			break;
		case 89:
			dmg = 0.95f;
			tagdmg = 3;
			break;
		case 90:
			dmg = 1.1f;
			kb = 0.9f;
			break;
		case 91:
			dmg = 0.95f;
			arpen = 10;
			break;
		case 92:
			dmg = 0.7f;
			break;
		case 93:
			kb = 0.75f;
			break;
		case 94:
			dmg = 0.85f;
			kb = 0.9f;
			break;
		case 95:
			arpen = 25;
			break;
		case 96:
			tagdmg = 5;
			break;
		case 97:
			kb = 1.25f;
			break;
		case 84:
			kb = 1.17f;
			dmg = 1.17f;
			crt = 8;
			break;
		case 36:
			crt = 3;
			break;
		case 37:
			dmg = 1.1f;
			crt = 3;
			kb = 1.1f;
			break;
		case 38:
			kb = 1.15f;
			break;
		case 53:
			dmg = 1.1f;
			break;
		case 54:
			kb = 1.15f;
			break;
		case 55:
			kb = 1.15f;
			dmg = 1.05f;
			break;
		case 59:
			kb = 1.15f;
			dmg = 1.15f;
			crt = 5;
			break;
		case 60:
			dmg = 1.15f;
			crt = 5;
			break;
		case 61:
			crt = 5;
			break;
		case 39:
			dmg = 0.7f;
			kb = 0.8f;
			break;
		case 40:
			dmg = 0.85f;
			break;
		case 56:
			kb = 0.8f;
			break;
		case 41:
			kb = 0.85f;
			dmg = 0.9f;
			break;
		case 57:
			kb = 0.9f;
			dmg = 1.18f;
			break;
		case 42:
			spd = 0.9f;
			break;
		case 43:
			dmg = 1.1f;
			spd = 0.9f;
			break;
		case 44:
			spd = 0.9f;
			crt = 3;
			break;
		case 45:
			spd = 0.95f;
			break;
		case 46:
			crt = 3;
			spd = 0.94f;
			dmg = 1.07f;
			break;
		case 47:
			spd = 1.15f;
			break;
		case 48:
			spd = 1.2f;
			break;
		case 49:
			spd = 1.08f;
			break;
		case 50:
			dmg = 0.8f;
			spd = 1.15f;
			break;
		case 51:
			kb = 0.9f;
			spd = 0.9f;
			dmg = 1.05f;
			crt = 2;
			break;
		}
		value = 1f * dmg * (2f - spd) * (2f - mcst) * size * kb * shtspd * (1f + (float)crt * 0.02f) * (1f + (float)arpen * 0.015f) * (1f + (float)tagdmg * 0.03f);
		if (rolledPrefix == 62 || rolledPrefix == 69 || rolledPrefix == 73 || rolledPrefix == 77)
		{
			value *= 1.05f;
		}
		if (rolledPrefix == 63 || rolledPrefix == 70 || rolledPrefix == 74 || rolledPrefix == 78 || rolledPrefix == 67)
		{
			value *= 1.1f;
		}
		if (rolledPrefix == 64 || rolledPrefix == 71 || rolledPrefix == 75 || rolledPrefix == 79 || rolledPrefix == 66)
		{
			value *= 1.15f;
		}
		if (rolledPrefix == 65 || rolledPrefix == 72 || rolledPrefix == 76 || rolledPrefix == 80 || rolledPrefix == 68)
		{
			value *= 1.2f;
		}
		if (dmg != 1f && Math.Round((float)damage * dmg) == (double)damage)
		{
			return false;
		}
		if (spd != 1f && Math.Round((float)useAnimation * spd) == (double)useAnimation)
		{
			return false;
		}
		if (mcst != 1f && Math.Round((float)mana * mcst) == (double)mana)
		{
			return false;
		}
		if (kb != 1f && knockBack == 0f)
		{
			return false;
		}
		return true;
	
	}
	public float BestPrefixValue()
{
	
		int[] rollablePrefixes = GetRollablePrefixes();
		if (rollablePrefixes == null)
		{
			return 0f;
		}
		float num = 0f;
		int[] array = rollablePrefixes;
		foreach (int rolledPrefix in array)
		{
			if (TryGetPrefixStatMultipliersForItem(rolledPrefix, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var val))
			{
				num = Math.Max(num, val);
			}
		}
		return num;
	
	}
	public int[] GetRollablePrefixes()
{
	
		_ = type;
		if (PrefixLegacy.ItemSets.SwordsHammersAxesPicks[type])
		{
			return PrefixLegacy.Prefixes.PrefixesForSwords;
		}
		if (PrefixLegacy.ItemSets.SpearsMacesChainsawsDrillsPunchCannon[type])
		{
			return PrefixLegacy.Prefixes.PrefixesForSpears;
		}
		if (PrefixLegacy.ItemSets.GunsBows[type])
		{
			return PrefixLegacy.Prefixes.PrefixesForGunsBows;
		}
		if (PrefixLegacy.ItemSets.Magic[type])
		{
			return PrefixLegacy.Prefixes.PrefixesForMagic;
		}
		if (PrefixLegacy.ItemSets.Summon[type])
		{
			return PrefixLegacy.Prefixes.PrefixesForSummons;
		}
		if (PrefixLegacy.ItemSets.BoomerangsChakrams[type])
		{
			return PrefixLegacy.Prefixes.PrefixesForBoomeransAndChakrums;
		}
		if (PrefixLegacy.ItemSets.ItemsThatCanHaveLegendary2[type])
		{
			return PrefixLegacy.Prefixes.PrefixesForBoomeransAndChakrums_TerrarianYoyo;
		}
		if (IsAPrefixableAccessory())
		{
			return PrefixLegacy.Prefixes.PrefixesForAccessories;
		}
		return null;
	
	}
	private bool RollAPrefix(UnifiedRandom random, ref int rolledPrefix)
{
	
		int[] rollablePrefixes = GetRollablePrefixes();
		if (rollablePrefixes == null)
		{
			return false;
		}
		rolledPrefix = rollablePrefixes[random.Next(rollablePrefixes.Length)];
		return true;
	
	}
	public bool IsAPrefixableAccessory()
{
	
		if (accessory && !vanity)
		{
			return ItemID.Sets.CanGetPrefixes[type];
		}
		return false;
	
	}
}
