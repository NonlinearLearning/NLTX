using System;

namespace Terraria.Player.Combat;

public static class PlayerArmorAndCombatEffectsSystem
{
    public static void Rebuild(
        in PlayerArmorAndCombatEffectsRebuildInput input,
        PlayerArmorAndCombatEffectsComponent effects)
    {
        ArgumentNullException.ThrowIfNull(effects);

        float thorns = 0f;
        if (input.ThornsBuffActive)
        {
            thorns = 1f;
        }
        else if (input.DryadWardActive)
        {
            thorns = 0.5f;
        }

        if (input.TurtleSetBonusActive)
        {
            thorns = 1f;
        }

        effects.Thorns = thorns;
        effects.TurtleArmor = input.TurtleArmorEquipped;
        effects.TurtleThorns = input.TurtleSetBonusActive;
        effects.CactusThorns = input.CactusSetBonusActive;
        effects.SpiderArmor = input.SpiderArmorEquipped;
        effects.AnglerSetSpawnReduction = input.AnglerSetBonusActive;
    }
}
