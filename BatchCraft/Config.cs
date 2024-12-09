using BepInEx.Configuration;
using BepInEx.Logging;

namespace FastFabricate;

internal class Config
{
    public static Config Instance { get; private set; }

    public ManualLogSource Logger { get; }

    public ConfigFile ConfigFile { get; }

    /*
     * Configs
     */

    public ConfigEntry<bool> ModEnabled { get; }

    public ConfigEntry<bool> InstantCrafting { get; }

    public ConfigEntry<float> FabricationDelayMultiplier { get; }

    public Config(ManualLogSource logger, ConfigFile configFile)
    {
        Instance = this;
        Logger = logger;
        ConfigFile = configFile;

        ModEnabled = configFile.Bind("General", 
            "ModEnabled", 
            true, 
            "Specifies whether the mod should be active or not.");

        InstantCrafting = configFile.Bind("Fabrication",
            "InstantCrafting",
            true,
            "Allows for fabrication to complete instantaneously.");

        FabricationDelayMultiplier = configFile.Bind("Fabrication",
            "FabricationDelayMultiplier",
            1f,
            "Multiplied with an items fabrication time. (fabrication time * multiplier = new fabrication time)");
    }
}
