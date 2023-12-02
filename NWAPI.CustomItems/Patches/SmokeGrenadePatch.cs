using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables.Scp244;
using InventorySystem.Items.Usables.Scp244.Hypothermia;
using NWAPI.CustomItems.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NWAPI.CustomItems.Patches
{
    /// <summary>
    /// Prevents damage from hypotermia effect.
    /// </summary>
    [HarmonyPatch(typeof(Scp244DeployablePickup), nameof(Scp244DeployablePickup.FogPercentForPoint))]
    public class SmokeGrenadePatch
    {
        private static bool Prefix(Scp244DeployablePickup __instance, ref float __result)
        {
            if (SmokeGrenade.preventHypotermia.Contains(__instance.NetworkInfo.Serial))
            {
                __result = 0;
                return false;
            }
            return true;
        }
    }
}
