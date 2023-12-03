using InventorySystem.Items.Usables.Scp244;
using MEC;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Extensions;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Features.Attributes;
using NWAPI.CustomItems.API.Spawn;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Items;
using PluginAPI.Events;
using System.Collections.Generic;
using System.ComponentModel;
using Utils;
using YamlDotNet.Serialization;

namespace NWAPI.CustomItems.Items
{
    [CustomItem]
    public class SmokeGrenade : CustomGrenade
    {
        [YamlIgnore]
        public static SmokeGrenade Instance;

        /// <inheritdoc/>
        public override bool ExplodeOnCollision { get; set; } = true;

        /// <inheritdoc/>
        public override float FuseTime { get; set; } = 2f;

        /// <inheritdoc/>
        public override uint Id { get; set; } = 10;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Smoke grenade";

        /// <inheritdoc/>
        public override string Description { get; set; } = "When this grenade explodes, it raises a fog of smoke for a certain period of time.";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 2.3f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.GrenadeFlash;

        /// <inheritdoc/>
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 3,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new()
                {
                    Chance = 100,
                    Location = SpawnLocationType.InsideLocker,
                    LockerZone = MapGeneration.FacilityZone.HeavyContainment,
                },
                new()
                {
                    Chance = 100,
                    Location = SpawnLocationType.InsideLocker,
                    LockerZone = MapGeneration.FacilityZone.HeavyContainment,
                },
                new()
                {
                    Chance = 100,
                    Location = SpawnLocationType.InsideLocker,
                    LockerZone = MapGeneration.FacilityZone.HeavyContainment,
                },
            },
        };

        /// <summary>
        /// Gets or sets the smoke effect duration.
        /// </summary>
        [Description("For how many seconds will the fog remain active?")]
        public float SmokeDuration { get; set; } = 30f;

        //private HashSet<ushort> blockPickup = new();

        /// <summary>
        /// Gets the hashset of <see cref="Scp244DeployablePickup"/>'s for preventing to apply <see cref="InventorySystem.Items.Usables.Scp244.Hypothermia.Hypothermia"/>
        /// </summary>
        [YamlIgnore]
        public static HashSet<ushort> preventHypotermia = new();

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
        private void OnWaitingForPlayerss(WaitingForPlayersEvent _)
        {
            preventHypotermia.Clear();
        }

        /// <inheritdoc/>
        public override bool OnExploding(GrenadeExplodedEvent ev)
        {
            var item = ItemPickup.Create(ItemType.SCP244a, ev.Position, default);

            if (item is null)
                return false;

            var scp244 = item.OriginalObject as Scp244DeployablePickup;


            if (scp244 != null)
            {
                scp244.State = Scp244State.Active;
            }

            item.OriginalObject.ChangeScale(new(0.1f, 0.1f, 0.1f));
            preventHypotermia.Add(item.Serial);
            item.Spawn();
            LateRemove(item);
            item.IsLocked = true;

            ExplosionUtils.ServerSpawnEffect(ev.Grenade.transform.position, ev.Grenade.Info.ItemId);
            ev.Grenade.DestroySelf();
            return false;
        }

        private void LateRemove(ItemPickup item)
        {
            Timing.CallDelayed(SmokeDuration, () =>
            {
                if (item.OriginalObject != null)
                {
                    preventHypotermia.Remove(item.Serial);
                    item.Destroy();
                }
            });
        }
    }
}
