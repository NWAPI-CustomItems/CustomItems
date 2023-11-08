using CustomPlayerEffects;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PlayerRoles.PlayableScps.Scp096;
using PluginAPI.Core.Attributes;
using PluginAPI.Core;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWAPI.CustomItems.API.Extensions.ScpRoles;
using NWAPI.CustomItems.API.Extensions;
using PlayerStatsSystem;
using System.Xml.Linq;
using CustomItems;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class LethalInjection : CustomItem
    {
        public static LethalInjection Instance;

        /// <inheritdoc />
        public override uint Id { get; set; } = 4;

        /// <inheritdoc />
        public override string Name { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.LethalInjection.Name;

        /// <inheritdoc />
        public override string Description { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.LethalInjection.Description;

        /// <inheritdoc />
        public override float Weight { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.LethalInjection.Weight;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.LethalInjection.ModelType;

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
