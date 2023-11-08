using NWAPI.CustomItems.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomItems
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true;

        public bool DebugMode { get; set; } = false;

        public CustomItemConfigs CustomItemConfigs { get; set; } = new();
    }

    public class CustomItemConfigs
    {
        public GrenadeLauncherConfig GrenadeLauncher { get; set; } = new();

        public AntiMemeticPillsConfig AntiMemeticPills { get; set; } = new();

        public EscapeCoinConfig EscapeCoin { get; set; } = new();

        public LethalInjectionConfig LethalInjection { get; set; } = new();

        public MimicHatConfig MimicHat { get; set; } = new();

        public SniperRifleConfig SniperRifle { get; set; } = new();

        public TranquilizerGunConfig TranquilizerGun { get; set; } = new();
    }
}
