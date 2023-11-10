using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using System.Collections.Generic;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class AnomalousArmor : CustomArmor
    {
        public static AnomalousArmor Instance;

        /// <inheritdoc/>
        public override uint Id { get; set; } = 9;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Anomalous armor";

        /// <inheritdoc/>
        public override string Description { get; set; } = "anomalous armor that improves movement and stamina at the cost of removing protection.";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 2f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.ArmorLight;

        /// <inheritdoc/>
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new()
                {
                   Chance = 22,
                   Location = SpawnLocationType.InsideLczArmory,
                },
            },
        };

        /// <inheritdoc/>
        public override int HelmetEfficacy { get; set; } = -30;

        /// <inheritdoc/>
        public override int VestEfficacy { get; set; } = -30;

        /// <inheritdoc/>
        public override float StaminaUseMultiplier { get; set; } = 0.5f;

        /// <inheritdoc/>
        public override float StaminaRegenMultiplier { get; set; } = 0.3f;

        /// <inheritdoc/>
        public override void SubscribeEvents()
        {
            base.SubscribeEvents();

            Instance ??= this;
            PluginAPI.Events.EventManager.RegisterEvents(Plugin.Instance, Instance);
        }

        /// <inheritdoc/>
        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            PluginAPI.Events.EventManager.UnregisterEvents(Plugin.Instance, Instance);
        }
    }
}
