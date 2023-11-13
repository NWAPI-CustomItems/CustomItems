using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Features.Attributes;
using NWAPI.CustomItems.API.Spawn;
using PlayerStatsSystem;
using PluginAPI.Events;
using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace NWAPI.CustomItems.Items
{
    [CustomItem]
    public class SniperRifle : CustomWeapon
    {
        [YamlIgnore]
        public static SniperRifle Instance;

        /// <inheritdoc/>
        public override float Damage { get; set; } = 30;

        /// <inheritdoc/>
        public override uint Id { get; set; } = 5;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Sniper Rifle";

        /// <inheritdoc/>
        public override string Description { get; set; } = "An E-11 modified to function as a Sniper.";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 6f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.GunE11SR;

        /// <inheritdoc/>
        public override byte ClipSize { get; set; } = 2;

        /// <inheritdoc/>
        public override uint AttachmentsCode { get; set; } = 19146000;

        /// <inheritdoc/>
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 2,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new()
                {
                    Chance = 100,
                    Location = SpawnLocationType.InsideHid,
                },
                new()
                {
                    Chance = 40,
                    Location = SpawnLocationType.InsideHczArmory,
                },
            },
        };

        /// <summary>
        /// Gets or sets the amount of damage multiplier for Humans.
        /// </summary>
        [Description("the amount of damage multiplier for Humans.")]
        public float DamageMultiplier { get; set; } = 3.1f;

        /// <summary>
        /// Gets or sets the amount of damage multiplier for Scps.
        /// </summary>
        [Description("the amount of damage multiplier for Scps.")]
        public float DamageMultiplierToScps { get; set; } = 8.2f;

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

        protected override void OnHurting(PlayerDamageEvent ev)
        {
            if (ev.Player is null || !ev.Player.IsReady || !Check(ev.Player.CurrentItem) || ev.Target is null || !ev.Target.IsReady)
                return;

            if (ev.DamageHandler is FirearmDamageHandler dmg)
            {
                if (!FriendlyFire && ev.Target.Team == ev.Player.Team)
                {
                    dmg.Damage = 0;
                }
                else
                {
                    dmg.Damage = Damage;

                    if (ev.Target.IsSCP)
                    {
                        // this only works for SCP-049-2
                        if (dmg.Hitbox == HitboxType.Headshot)
                            dmg.Damage = 50;

                        dmg.Damage *= DamageMultiplierToScps;

                        ev.Target.SendConsoleMessage($" You have been damaged by the custom item {Name}");
                    }
                    else
                    {

                        if (dmg.Hitbox == HitboxType.Headshot)
                            dmg.Damage = 50;

                        dmg.Damage *= DamageMultiplier;

                        ev.Target.SendConsoleMessage($" You have been damaged by the custom item {Name}");
                    }
                }
            }
        }
    }
}
