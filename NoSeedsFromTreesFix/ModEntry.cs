using Harmony;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
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
            //https://github.com/kjryanreeves/NoSeedsFromTreesFix.git
            var harmony = HarmonyInstance.Create("com.github.kirbylink.noseedsfromtreesfix");
            harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Farmer))]
    [HarmonyPatch("getEffectiveSkillLevel")]
    public static class PatchGetEffectiveSkillLevel
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*  Current Code Issue: Effective level calculation results in 0 or negative level causing
                calls to getEffectiveSkillLevel to return an incorrect level value

                Possible Fix: Line 1661 of Farmer.cs should be a simple decrement or -= 1 statement
                instead of using the newLevels.Y value

                Algorithm:
                1. Find load of numArray[this.newLevels[index].X] onto stack (ldind.i4)
                2. Store that index + 1, continue to find sub
                3. Store sub index - 1
                4. Set first index to a load of 1 on stack (ldc.i4.1)
                5. Remove remainder of indexes up to sub index  - 1
            */

            var codesInstList = new List<CodeInstruction>(instructions);
            int loadPlusOneIndex = -1;
            int subMinusOneIndex = -1;
            bool foundRange = false;

            for (int i = 0; i < codesInstList.Count; i++)
            {
                if (codesInstList[i].opcode == OpCodes.Ldind_I4)
                {
                    loadPlusOneIndex = i + 1;
                }
                else if (codesInstList[i].opcode == OpCodes.Sub)
                {
                    subMinusOneIndex = i - 1;
                    foundRange = true;
                    break;
                }
            }

            if (foundRange)
            {
                codesInstList[loadPlusOneIndex].opcode = OpCodes.Ldc_I4_1;
                codesInstList.RemoveRange(loadPlusOneIndex + 1, subMinusOneIndex - loadPlusOneIndex - 1);
            }

            return codesInstList.AsEnumerable();
        }
    }
}
