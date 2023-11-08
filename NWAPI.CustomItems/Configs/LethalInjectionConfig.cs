using NWAPI.CustomItems.Features.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWAPI.CustomItems.Configs
{
    public class LethalInjectionConfig : CustomItemConfig
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Lethal injection";

        /// <inheritdoc />
        public override string Description { get; set; } = "Anomalous injection that when applied to your body will instantly decompose but will cause the SCP-096 which you were the target to calm down.";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.3f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.Adrenaline;
    }
}
