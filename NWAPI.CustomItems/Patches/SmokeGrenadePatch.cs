using HarmonyLib;
using InventorySystem.Items.Usables.Scp244;
using NWAPI.CustomItems.Items;

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
