using CustomItems.Items;
using NWAPI.CustomItems.Items;
using System.Collections.Generic;
using System.ComponentModel;

namespace CustomItems
{
    public class Config
    {
        [Description("This plugin is enabled ?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enabled some log.debug in the code.")]
        public bool DebugMode { get; set; } = false;
    }

    /// <summary>
    /// All custom items to be registered.
    /// </summary>
    public class CustomItemConfigs
    {
        /// <summary>
        /// Gets the list of escape Coins.
        /// </summary>
        [Description("The list of escape coins.")]
        public List<EscapeCoin> EscapeCoins { get; set; } = new()
        {
            new EscapeCoin(),
        };

        /// <summary>
        /// Gets the list of grenade launchers.
        /// </summary>
        [Description("The list of grenade launchers.")]
        public List<GrenadeLauncher> GrenadeLaunchers { get; set; } = new()
        {
            new GrenadeLauncher(),
        };

        /// <summary>
        /// Gets the list of anti-memetic pills.
        /// </summary>
        [Description("The list of anti-memetic pills.")]
        public List<AntiMemeticPills> AntiMemeticPills { get; set; } = new()
        {
            new AntiMemeticPills(),
        };

        /// <summary>
        /// Gets the list of lethal injections.
        /// </summary>
        [Description("The list of lethal injections.")]
        public List<LethalInjection> LethalInjections { get; set; } = new()
        {
            new LethalInjection(),
        };

        /// <summary>
        /// Gets the list of mimic hats.
        /// </summary>
        [Description("The list of mimic hats.")]
        public List<MimicHat> MimicHats { get; set; } = new()
        {
            new MimicHat(),
        };

        /// <summary>
        /// Gets the list of sniper rifles.
        /// </summary>
        [Description("The list of sniper rifles.")]
        public List<SniperRifle> SniperRifles { get; set; } = new()
        {
            new SniperRifle(),
        };

        /// <summary>
        /// Gets the list of tranquilizer guns.
        /// </summary>
        [Description("The list of tranquilizer guns.")]
        public List<TranquilizerGun> TranquilizerGuns { get; set; } = new()
        {
            new TranquilizerGun(),
        };

        /// <summary>
        /// Gets the list of anomalous armors.
        /// </summary>
        [Description("The list of anomalous armors.")]
        public List<AnomalousArmor> AnomalousArmors { get; set; } = new()
        {
            new AnomalousArmor(),
        };

        /// <summary>
        /// Gets the list of zabralo armors.
        /// </summary>
        [Description("The list of zabralo armors.")]
        public List<ZabraloArmor> ZabraloArmors { get; set; } = new()
        {
            new ZabraloArmor(),
        };
    }

}
