using CustomItems;
using NWAPI.CustomItems.Features.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWAPI.CustomItems.Configs
{
    public class EscapeCoinConfig : CustomItemConfig
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Escape coin";

        /// <inheritdoc />
        public override string Description { get; set; } = "Flipping this coin in the pocket dimension will you have the chance to exit at once or die instantly.";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.1f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.Coin;
    }
}
