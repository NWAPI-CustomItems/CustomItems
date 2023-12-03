using CustomPlayerEffects;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using YamlDotNet.Serialization;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class ZabraloArmor : CustomArmor
    {
        [YamlIgnore]
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
        public override float StaminaUseMultiplier { get; set; } = 8f;

        /// <inheritdoc/>
        public override float StaminaRegenMultiplier { get; set; } = 0.002f;

        /// <summary>
        /// Gets or sets the damage reduction given for the armor owner.
        /// </summary>
        [Description("How much damage resistance will be given to the player with the armor")]
        public byte DamageReduction { get; set; } = 150;

        private HashSet<Player> _PlayersWithArmor = new();

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

        public override void OnArmorPickedup(PlayerPickupArmorEvent ev)
        {
            base.OnArmorPickedup(ev);

            AddBuff(ev.Player);
        }

        [PluginEvent]
        public void OnArmorDropped(PlayerDroppedItemEvent ev)
        {
            if (!Check(ev.Item))
                return;

            RemoveBuff(ev.Player);
        }

        [PluginEvent]
        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            _PlayersWithArmor.Clear();
        }

        private void AddBuff(Player player)
        {
            if (!_PlayersWithArmor.Contains(player))
            {
                _PlayersWithArmor.Add(player);

                var effect = player.EffectsManager.EnableEffect<DamageReduction>();

                effect.Intensity = DamageReduction;
            }

        }

        private void RemoveBuff(Player player)
        {
            if (player.IsAlive)
            {
                player.EffectsManager.DisableEffect<DamageReduction>();
            }

            _PlayersWithArmor.Remove(player);
        }
    }
}
