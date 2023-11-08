using NWAPI.CustomItems.API.Spawn;
using NWAPI.CustomItems.Features.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWAPI.CustomItems.Configs
{
    public class GrenadeLauncherConfig : CustomItemConfig
    {
        public float Damage { get; set; } = 0;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Grenade Launcher";

        /// <inheritdoc/>
        public override string Description { get; set; } = "Shot grenades and you need grenades to reloaded, all types of grenades can be used.";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.49f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.GunLogicer;

        public byte ClipSize { get; set; } = 2;

        public uint AttachmentsCode { get; set; } = 5252;

        public bool IgnoreCustomGrenade { get; set; } = true;
    }
}
