using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Spawn;
using NWAPI.CustomItems.Features.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWAPI.CustomItems.Configs
{
    public class TranquilizerGunConfig : CustomItemConfig
    {
        /// <inheritdoc/>
        public float Damage { get; set; } = 5;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Tranquilizer gun";

        /// <inheritdoc/>
        public override string Description { get; set; } = "A USP modified to fire tranquilizer darts, very effective against humans but not very effective against SCPs.";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 6f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.GunCOM18;

        /// <inheritdoc/>
        public byte ClipSize { get; set; } = 1;

        /// <inheritdoc/>
        public uint AttachmentsCode { get; set; } = 1170;

        /// <summary>
        /// Gets or sets the percent chance an SCP will resist being tranquilized. This has no effect if ResistantScps is false.
        /// </summary>
        public int ScpResistChance { get; set; } = 65;

        /// <summary>
        /// Gets or sets the amount of time a successful tranquilization lasts for.
        /// </summary>
        public float Duration { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the exponential modifier used to determine how much time is removed from the effect, everytime a player is tranquilized, they gain a resistance to further tranquilizations, reducing the duration of future effects.
        /// </summary>
        public float ResistanceModifier { get; set; } = 1.1f;

        /// <summary>
        /// Gets or sets a value indicating how often player resistances are reduced.
        /// </summary>
        public float ResistanceFalloffDelay { get; set; } = 60f;
    }
}
