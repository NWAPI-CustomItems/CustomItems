using CustomItems;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Extensions;
using NWAPI.CustomItems.API.Extensions.ScpRoles;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PlayerRoles.PlayableScps.Scp096;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using YamlDotNet.Serialization;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class LethalInjection : CustomItem
    {
        [YamlIgnore]
        public static LethalInjection Instance;

        /// <inheritdoc />
        public override uint Id { get; set; } = 4;

        /// <inheritdoc />
        public override string Name { get; set; } = "Lethal injection";

        /// <inheritdoc />
        public override string Description { get; set; } = "Anomalous injection that when applied to your body will instantly decompose but will cause the SCP-096 which you were the target to calm down.";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.3f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.Adrenaline;

        /// <inheritdoc />
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 3,
            DynamicSpawnPoints = new()
            {
                new()
                {
                    Chance = 100,
                    Location = SpawnLocationType.Inside096,
                },
                new()
                {
                    Chance = 30,
                    Location = SpawnLocationType.InsideIntercom
                },
                new()
                {
                    Chance = 50,
                    Location = SpawnLocationType.InsideSurfaceNuke
                }
            },
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
                if (player.Role != PlayerRoles.RoleTypeId.Scp096 || player.RoleBase is not Scp096Role role || !role.HasTarget(ev.Player))
                    continue;

                role.EndRage();
            }

            ev.Player.Kill(new UniversalDamageHandler(-1, DeathTranslations.Poisoned));
            ev.Player.SendConsoleMessage($" You were killed by the custom item {Name}");
        }
    }
}
