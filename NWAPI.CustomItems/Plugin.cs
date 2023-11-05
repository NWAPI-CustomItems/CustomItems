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

        [PluginEntryPoint("NWAPI.CustomItems", Version, "Boop", "SrLicht")]
        private void OnLoad()
        {
            Instance = this;

            if (!Config.IsEnabled)
                return;

            try
            {
                CustomItem.RegisterItems();
            }
            catch (Exception e)
            {
                Log.Error($"Error on trying to register items: {e.Message}");
            }
        }
    }
}
