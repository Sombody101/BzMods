using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace FastFabricate;

[BepInPlugin(GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GUID = "com.Sombody101.FastFabricate";

    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        _ = new Config(Logger, Config);

        Harmony harmony = new(GUID);
        harmony.PatchAll();
    }
}
