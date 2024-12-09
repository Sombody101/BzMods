using Common;
using HarmonyLib;

namespace FastFabricate;

internal static class GuiConfig
{
    public const string MODS = "Mods";
    private static int? modTabIndex;

    [HarmonyPatch(typeof(uGUI_TabbedControlsPanel))]
    [HarmonyPatch(nameof(uGUI_TabbedControlsPanel.AddTab))]
    public static class AddTabPatch
    {
        [HarmonyPostfix]
        public static void AssignModsTabIndex(uGUI_TabbedControlsPanel __instance, int __result, string label)
        {
            if (label is MODS)
            {
                modTabIndex = __result;
            }
        }
    }

    [HarmonyPatch(typeof(uGUI_OptionsPanel))]
    [HarmonyPatch("AddTabs")]
    public static class AddTabsPatch
    {
        [HarmonyPostfix]
        public static void EnsureModsTabAdded(uGUI_OptionsPanel __instance)
        {
            if (modTabIndex is null)
            {
                _ = __instance.AddTab(MODS);
            }
        }

        [HarmonyPostfix]
        public static void AddDrillSection(uGUI_OptionsPanel __instance)
        {
            if (modTabIndex is not int tab)
            {
                return;
            }

            Config config = Config.Instance;

            __instance.AddHeading(tab, "Fast Fabricate");

            _ = __instance.AddToggleOption(tab, "Instant Fabrication", config.InstantCrafting.Value, (value) =>
            {
                config.InstantCrafting.Value = value;
                $"Instant crafting {(value ? "enabled" : "disabled")} by user.".LogDebug();
            }, "Enable or disable instant fabrication. (Removes the delay for fabricating items)");

            _ = __instance.AddSliderOption(tab, 
                "Fabrication Multiplier", 
                config.FabricationDelayMultiplier.Value, 
                0.01f, 
                5f,
                (float)config.FabricationDelayMultiplier.DefaultValue, 
                0.01f, 
                (value) => config.FabricationDelayMultiplier.Value = value, 
                SliderLabelMode.Float, 
                "0.00",
                "Multiplied with an items fabrication time.\n(fabrication time * multiplier = new fabrication time)");
        }
    }

    [HarmonyPatch(typeof(uGUI_OptionsPanel))]
    [HarmonyPatch("OnDisable")]
    public static class OnDisablePatch
    {
        [HarmonyPostfix]
        public static void SaveConfig(uGUI_OptionsPanel __instance)
        {
            if (modTabIndex is not null)
            {
                Config.Instance.Logger.LogInfo("Saving configuration.");
                Config.Instance.ConfigFile.Save();
            }
        }

        [HarmonyPostfix]
        public static void DeassignModsTabIndex()
        {
            modTabIndex = null;
        }
    }
}
