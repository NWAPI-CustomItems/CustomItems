using NWAPI.CustomItems.Features.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWAPI.CustomItems.Configs
{
    public class SniperRifleConfig : CustomItemConfig
    {
        /// <inheritdoc/>
        public  float Damage { get; set; } = 30;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Sniper Rifle";

        /// <inheritdoc/>
        public override string Description { get; set; } = "An E-11 modified to function as a Sniper.";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 6f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.GunE11SR;

        /// <inheritdoc/>
        public  byte ClipSize { get; set; } = 2;

        /// <inheritdoc/>
        public  uint AttachmentsCode { get; set; } = 19146000;

        /// <summary>
        /// Gets or sets the amount of damage multiplier for Humans.
        /// </summary>
        public float DamageMultiplier { get; set; } = 3.1f;

        /// <summary>
        /// Gets or sets the amount of damage multiplier for Scps.
        /// </summary>
        public float DamageMultiplierToScps { get; set; } = 8.2f;
    }
}
