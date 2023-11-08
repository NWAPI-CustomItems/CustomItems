using NWAPI.CustomItems.Features.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWAPI.CustomItems.Configs
{
    public class MimicHatConfig : CustomItemConfig
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Mimic hat";

        /// <inheritdoc />
        public override string Description { get; set; } = "Wearing this hat will change your appearance to that of a random live SCP for a period of time.";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.1f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.SCP268;

        public float Duration { get; set; } = 15f;

        public float Cooldown { get; set; } = 40f;
    }
}
