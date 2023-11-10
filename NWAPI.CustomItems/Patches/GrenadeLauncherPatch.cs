using CustomItems.Items;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using NWAPI.CustomItems.API.Components;

namespace NWAPI.CustomItems.Patches
{
    [HarmonyPatch(typeof(TimeGrenade), nameof(TimeGrenade.ServerActivate))]
    public class GrenadeLauncherPatch
    {

        /// <summary>
        /// This method runs before ServerActivate and checks if a grenade should have the GrenadeExplodeOnCollision component.
        /// </summary>
        /// <param name="__instance">The instance of the TimeGrenade being activated.</param>
        public static void Prefix(TimeGrenade __instance)
        {
            if (GrenadeLauncher.GrenadesSerials.Contains(__instance.NetworkInfo.Serial))
            {
                if (__instance.NetworkInfo.ItemId == ItemType.SCP018 || __instance.NetworkInfo.ItemId == ItemType.SCP2176)
                    return;

                __instance.gameObject.AddComponent<ExplodeOnCollision>();
                return;
            }

        }
    }
}
