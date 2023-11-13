using CustomItems;
using CustomPlayerEffects;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Extensions.ScpRoles;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PlayerRoles.PlayableScps.Scp096;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using YamlDotNet.Serialization;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class AntiMemeticPills : CustomItem
    {
        [YamlIgnore]
        public static AntiMemeticPills Instance;

        /// <inheritdoc />
        public override uint Id { get; set; } = 2;

        /// <inheritdoc />
        public override string Name { get; set; } = "Anti-Memetic pills";

        /// <inheritdoc />
        public override string Description { get; set; } = "Pills that make you forget the face of SCP-096";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.2f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.Painkillers;

        /// <inheritdoc />
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 3,
            DynamicSpawnPoints = new()
            {
                new()
                {
                    Chance = 100,
                    Location = SpawnLocationType.Inside096
                },
                new()
                {
                    Chance = 80,
                    Location = SpawnLocationType.InsideLczWc
                },
                new()
                {
                    Chance = 40,
                    Location = SpawnLocationType.Inside330
                },
                new()
                {
                    Chance = 50,
                    Location = SpawnLocationType.InsideLczCafe
                },
                new()
                {
                    Chance = 30,
                    Location = SpawnLocationType.InsideGr18
                }
            }
        };

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
        public void OnItemUsed(PlayerUsedItemEvent ev)
        {
            if (!Check(ev.Item))
                return;

            Log.Debug($"{ev.Player.LogName} is using {Name}", EntryPoint.Instance.Config.DebugMode);
            foreach (var player in Player.GetPlayers())
            {
                if (player.Role != PlayerRoles.RoleTypeId.Scp096 || player.RoleBase is not Scp096Role role)
                    continue;

                role.RemoveTarget(ev.Player);
            }

            ev.Player.EffectsManager.EnableEffect<AmnesiaVision>(30, true);
            ev.Player.EffectsManager.EnableEffect<AmnesiaItems>(10, true);
        }
    }
}
