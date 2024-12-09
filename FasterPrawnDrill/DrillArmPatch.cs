using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace FasterPrawnDrill;

/*
 * https://github.com/datvm/SubnauticaBZMods/blob/master/LukeMods.FasterPrawnDrill/ExosuitDrillArmPatch.cs
 */

[HarmonyPatch(typeof(ExosuitDrillArm), "OnHit")]
internal static class DrillArmPatch
{
    private static readonly FieldInfo ExosuitField = typeof(ExosuitDrillArm).GetField("exosuit", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo DrillingField = typeof(ExosuitDrillArm).GetField("drilling", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPatch]
    public static void Prefix(ExosuitDrillArm __instance)
    {
        var config = DrillConfig.Instance;
        
        if (!config.ModEnabled.Value)
        { 
            return; 
        }

        var generalDamage = config.NonResourceOffset.Value;
        if (generalDamage <= 0)
        {
            return;
        }

        if (ExosuitField.GetValue(__instance) is not Exosuit suit)
        {
            config.Logger.LogError("Failed to get exosuit.");
            return;
        }

        if (!(suit.CanPilot() && suit.GetPilotingMode()))
        {
            return;
        }

        try
        {
            Vector3 position = Vector3.zero, __;
            GameObject closestObj = null;

            if (!UWE.Utils.TraceFPSTargetPosition(suit.gameObject, 5f, ref closestObj, ref position, out __)
                && (bool)DrillingField.GetValue(__instance))
            {
                return;
            }

            Drillable drillable = closestObj.FindAncestor<Drillable>();

            if (!drillable)
            {
                LiveMixin liveMixin = closestObj.FindAncestor<LiveMixin>();
                if (liveMixin)
                {
                    liveMixin.IsAlive();
                    liveMixin.TakeDamage(generalDamage, position, DamageType.Drill);

                    config.Logger.LogDebug($"Adding damage: {generalDamage}");
                }

                return;
            }

            var drillDamage = config.DrillDamageOffset.Value;
            var healths = drillable.health;

            config.Logger.LogDebug($"Hitting drillable with {healths.Length} healths.");

            for (int i = 0; i < healths.Length; i++)
            {
                var health = healths[i];
                if (health > drillDamage)
                {
                    config.Logger.LogDebug($"Drillable health reduced from healths[{i}] = ({health}), to {drillDamage}.");

                    healths[i] = drillDamage;
                }
            }
        }
        catch (Exception ex)
        {
            config.Logger.LogError(ex);
        }
    }
}
