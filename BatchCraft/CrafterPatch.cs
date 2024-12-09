using HarmonyLib;

namespace FastFabricate;

[HarmonyPatch]
internal static class CrafterPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CrafterLogic), "Update")]
    public static bool Prefix_CrafterLogic_Update(CrafterLogic __instance)
    {
        Config config = Config.Instance;

        if (!config.ModEnabled.Value)
        {
            return true;
        }

        // Overrides the user-set multiplier
        if (config.InstantCrafting.Value)
        {
            // Make the fabrication finish virtually instantaneously
            __instance.timeCraftingEnd = __instance.timeCraftingBegin + .01f;

            return true;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CrafterLogic), nameof(CrafterLogic.Craft))]
    public static void Postfix_CrafterLogic_Craft(CrafterLogic __instance)
    {
        Config config = Config.Instance;
        if (!config.ModEnabled.Value)
        {
            return;
        }

        // Multiply the craft time with the user-set multiplier.
        __instance.timeCraftingEnd = __instance.timeCraftingBegin + ((__instance.timeCraftingEnd - __instance.timeCraftingBegin) * config.FabricationDelayMultiplier.Value);
    }
}