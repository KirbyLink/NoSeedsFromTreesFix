using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NoSeedsfromTreeFix
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            //Harmony patcher
            //https://github.com/kirbylink/NoSeedsFromTreesFix.git
            var harmony = HarmonyInstance.Create("com.github.kirbylink.noseedsfromtreesfix");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }

    [HarmonyPatch(typeof(Farmer))]
    [HarmonyPatch("getEffectiveSkillLevel")]
    [HarmonyPatch(new Type[] { typeof(int) })]
    public static class PatchGetEffectiveSkillLevel
    {
        static void Postfix(Farmer __instance, ref int whichSkill, ref int __result)
        {
            /*  Current Code Issue: Effective level calculation results in 0 or negative level causing
                calls to getEffectiveSkillLevel to return an incorrect level value

                Possible Fix: Line 1661 of Farmer.cs should be a simple decrement or -= 1 statement
                instead of using the newLevels.Y value
            */
            if (whichSkill >= 0 && whichSkill <= 5)
            {
                int[] numArray = new int[6]
                {
                    (int) (NetFieldBase<int, NetInt>) __instance.farmingLevel,
                    (int) (NetFieldBase<int, NetInt>) __instance.fishingLevel,
                    (int) (NetFieldBase<int, NetInt>) __instance.foragingLevel,
                    (int) (NetFieldBase<int, NetInt>) __instance.miningLevel,
                    (int) (NetFieldBase<int, NetInt>) __instance.combatLevel,
                    (int) (NetFieldBase<int, NetInt>) __instance.luckLevel
                };

                for (int i = 0; i < __instance.newLevels.Count; ++i)
                    numArray[__instance.newLevels[i].X]--;
                __result = numArray[whichSkill];
            }
        }
    }
}
