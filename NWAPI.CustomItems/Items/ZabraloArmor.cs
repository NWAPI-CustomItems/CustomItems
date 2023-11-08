using InventorySystem.Items.Armor;
using MEC;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PlayerList;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class ZabraloArmor : CustomArmor
    {
        public static ZabraloArmor Instance;

        /// <inheritdoc/>
        public override uint Id { get; set; } = 8;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Zabralo armor";

        /// <inheritdoc/>
        public override string Description { get; set; } = "This armor make you super resistant to bullets, but consume a lot of stamina";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 20f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.ArmorHeavy;

        /// <inheritdoc/>
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new()
                {
                   Chance = 40,
                   Location = SpawnLocationType.Inside079Secondary,
                },
            },
        };

        /// <inheritdoc/>
        public override int HelmetEfficacy { get; set; } = 100;

        /// <inheritdoc/>
        public override int VestEfficacy { get; set; } = 95;

        /// <inheritdoc/>
        public override Vector3 Scale { get; set; } = new(1.3f, 1.3f, 1.3f);

        /// <inheritdoc/>
        public override float StaminaUseMultiplier { get; set; } = 5f;

        /// <inheritdoc/>
        public override float StaminaRegenMultiplier { get; set; } = 0.002f;

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

        [PluginEvent]
        public void OnTest(PlayerPickupArmorEvent ev)
        {
            var serial = ev.Item.NetworkInfo.Serial;

            Timing.CallDelayed(1, () =>
            {
                foreach(var item in ev.Player.Items.ToList())
                {
                    if (item.ItemSerial != serial)
                        continue;

                    if(item is BodyArmor armor)
                    {
                        Log.Info($"Armor {armor.StaminaModifierActive} | {armor.StaminaRegenMultiplier} | {armor.StaminaUsageMultiplier} | {armor.SprintingDisabled} | {armor.Weight} | {armor.HelmetEfficacy} | {armor.VestEfficacy}");
                    }
                }
            });
        }
    }
}
