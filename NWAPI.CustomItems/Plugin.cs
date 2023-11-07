using HarmonyLib;
using NWAPI.CustomItems.API.Features;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomItems
{
    public class Plugin
    {
        /// <summary>
        /// Gets the singleton instance of the plugin.
        /// </summary>
        public static Plugin Instance { get; private set; } = null!;

        /// <summary>
        /// Plugin config.
        /// </summary>
        [PluginConfig]
        public Config Config = null!;

        /// <summary>
        /// Gets the plugin version.
        /// </summary>
        public const string Version = "0.0.1";

        /// <summary>
        /// Gets the Harmony instance used for patching and unpatching.
        /// </summary>
        public static Harmony Harmony { get; private set; } = null!;

        /// <summary>
        /// Harmony id used to track assembly patchs.
        /// </summary>
        private static string HarmonyId = "";

        [PluginEntryPoint("NWAPI.CustomItems", Version, "Boop", "SrLicht")]
        private void OnLoad()
        {
            Instance = this;

            if (!Config.IsEnabled)
                return;

            HarmonyId = $"NWAPI.CustomItems.{Version}";
            Harmony = new(HarmonyId);

            try
            {
                Harmony.PatchAll();
            }
            catch (HarmonyException e)
            {
                Log.Error($"Error on patching: {e}");
            }

            try
            {
                CustomItem.RegisterItems();
            }
            catch (Exception e)
            {
                Log.Error($"Error on trying to register items: {e.Message}");
            }
        }

        [PluginUnload]
        private void UnLoad()
        {
            // Prevents unpatching all patches of the plugins by using HarmonyId.
            Harmony.UnpatchAll(HarmonyId);
            PluginAPI.Events.EventManager.UnregisterAllEvents(Instance);
        }
    }
}
