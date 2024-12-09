using BepInEx.Configuration;
using BepInEx.Logging;

namespace FasterPrawnDrill;

internal class DrillConfig
{
    private const string GENERAL = "General";

    public static DrillConfig Instance { get; set; }

    public ConfigFile ConfigFile { get; private set; }

    internal ManualLogSource Logger { get; }

    public ConfigEntry<int> DrillDamageOffset;

    public ConfigEntry<int> NonResourceOffset;

    public ConfigEntry<bool> ModEnabled;

    public DrillConfig(ManualLogSource logger, ConfigFile config)
    {
        Logger = logger;
        ConfigFile = config;

        DrillDamageOffset = config.Bind(GENERAL,
            "DrillDamageOffset",
            50,
            "The speed of the drill. 0 is the fastest, higher becomes slower. The game default is 200.");

        NonResourceOffset = config.Bind(GENERAL,
            "NonResourceOffset",
            16,
            "The amount of damage done to non-resource-pile entities (creatures). The game default is 4.");

        ModEnabled = config.Bind(GENERAL,
            "EnableMod",
            true,
            "Choose if the mod should be active.");

        if (DrillDamageOffset.Value < 0)
        {
            Logger.LogInfo($"Setting DrillDamageOffset from {DrillDamageOffset.Value} to 1.");
            DrillDamageOffset.Value = 1;
        }

        LogAllSettings();
    }

    public void SetConfigFile(ConfigFile configFile)
    {
        ConfigFile = configFile;
    }

    public void LogAllSettings()
    {
        Logger.LogInfo($"Active config:\r\n\tModEnabled:\t\t{ModEnabled.Value}" +
            $"\r\n\tDrillDamageOffset:\t{DrillDamageOffset.Value}" +
            $"\r\n\tNonResourceOffset:\t{NonResourceOffset.Value}");
    }
}
