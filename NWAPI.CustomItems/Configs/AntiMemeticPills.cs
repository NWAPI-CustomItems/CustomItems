using NWAPI.CustomItems.Features.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWAPI.CustomItems.Configs
{
    public class AntiMemeticPillsConfig : CustomItemConfig
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Anti-Memetic pills";

        /// <inheritdoc />
        public override string Description { get; set; } = "Pills that make you forget the face of SCP-096";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.2f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.Painkillers;
    }
}
