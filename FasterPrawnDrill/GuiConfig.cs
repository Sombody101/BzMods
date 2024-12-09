using HarmonyLib;

namespace FasterPrawnDrill;

internal static class GuiConfig
{
    public const string MODS = "Mods";

    static int? modTabIndex;

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
                __instance.AddTab(MODS);
            }
        }

        [HarmonyPostfix]
        public static void AddDrillSection(uGUI_OptionsPanel __instance)
        {
            if (modTabIndex is not int tab)
            {
                return;
            }

            DrillConfig config = DrillConfig.Instance;

            __instance.AddHeading(tab, "Faster PRAWN Drill");

            __instance.AddToggleOption(tab, "Enable Mod", config.ModEnabled.Value, (value) =>
            {
                config.ModEnabled.Value = value;
                config.Logger.LogInfo($"Fast PRAWN mod {(value ? "enabled" : "disabled")} by user.");
            }, "Enable or disable the mod.");

            // Having minValue anything <1 makes the resources indestructible
            __instance.AddSliderOption(tab, "Drill Damage Offset", config.DrillDamageOffset.Value, 1, 400, 200, 1, (value) =>
            {
                config.DrillDamageOffset.Value = (int)value;
            }, 
            SliderLabelMode.Int, "000", 
            "Change the drill damage offset (lower is faster, higher is slower, game default is 200)");

            __instance.AddSliderOption(tab, "Non-Resource Offset", config.NonResourceOffset.Value, -50, 400, 4, 1, (value) =>
            {
                config.NonResourceOffset.Value = (int)value;
            },
            SliderLabelMode.Int, "000",
            "Damage dealt to non-resource entities, like fish or sharks. (lower is weaker, game default is 4)");
        }
    }

    [HarmonyPatch(typeof(uGUI_OptionsPanel))]
    [HarmonyPatch("OnDisable")]
    public static class OnDisablePatch
    {
        [HarmonyPostfix]
        public static void SaveDrillConfig(uGUI_OptionsPanel __instance)
        {
            if (modTabIndex is not null)
            {
                DrillConfig.Instance.ConfigFile.Save();
                DrillConfig.Instance.LogAllSettings();
            }
        }

        [HarmonyPostfix]
        public static void DeassignModsTabIndex()
        {
            modTabIndex = null;
        }
    }
}
