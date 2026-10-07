using System;

namespace Terraria.Town;

[Flags]
public enum TownResidentCapabilities : byte
{
  None = 0,
  CanUseHousing = 1 << 0,
  CanOpenDialogue = 1 << 1,
  CanOfferCommerce = 1 << 2,
  CanParticipateInHappiness = 1 << 3,
}
