using CustomItems;
using CustomPlayerEffects;
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
using System.ComponentModel;
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

        [Description("When the player is killed this reason be show in the console of the player.")]
        public string DeathReason { get; set; } = " You were killed by the custom item Lethal injection";

        [Description("this hint will be send to the player when use the custom item when its not target of any SCP-096")]
        public string DamageReason { get; set; } = $"You used this item without being targeted by a SCP-096 so it only took half of your life.";

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

            bool endRage = false;
            Log.Debug($"{ev.Player.LogName} is using {Name}", EntryPoint.Instance.Config.DebugMode);

            foreach (var player in Player.GetPlayers())
            {
                if (player.Role != PlayerRoles.RoleTypeId.Scp096 || player.RoleBase is not Scp096Role role || !role.HasTarget(ev.Player))
                    continue;

                endRage = true;

                role.EndRage();
                player.EffectsManager.EnableEffect<Burned>(30).Intensity = 40;
            }


            if (!endRage)
            {
                ev.Player.Health /= 2;
                ev.Player.ReceiveHint(DamageReason, 10);
                return;
            }

            if (ev.Player.EffectsManager.TryGetEffect<AntiScp207>(out var statusEffect) && statusEffect.IsEnabled)
            {
                statusEffect.IsEnabled = false;
                ev.Player.Health = 1;
            }
            else
            {
                ev.Player.Kill(new UniversalDamageHandler(-1, DeathTranslations.Poisoned));
                ev.Player.SendConsoleMessage(DeathReason);
            }
        }
    }
}
