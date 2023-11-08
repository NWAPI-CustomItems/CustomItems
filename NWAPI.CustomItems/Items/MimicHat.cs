using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PlayerRoles.PlayableScps.Scp096;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Core;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWAPI.CustomItems.API.Extensions;
using NWAPI.CustomItems.API.Extensions.ScpRoles;
using CustomPlayerEffects;
using PlayerRoles;
using MEC;
using System.Data;
using InventorySystem.Items.Usables;
using UnityEngine;
using CustomItems;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class MimicHat : CustomItem
    {
        public static MimicHat Instance;

        /// <inheritdoc />
        public override uint Id { get; set; } = 7;

        /// <inheritdoc />
        public override string Name { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.MimicHat.Name;

        /// <inheritdoc />
        public override string Description { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.MimicHat.Description;

        /// <inheritdoc />
        public override float Weight { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.MimicHat.Weight;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.MimicHat.ModelType;

        /// <inheritdoc />
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            DynamicSpawnPoints = new()
            {
                new()
                {
                    Chance = 60,
                    Location = SpawnLocationType.InsideNukeArmory,
                },
            }
        };

        public float Duration { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.MimicHat.Duration;

        public float Cooldown { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.MimicHat.Cooldown;

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

        private Dictionary<string, RoleTypeId> _oldRoles = new();
        private List<RoleTypeId> _scpRolestypes = new List<RoleTypeId>
        {
            RoleTypeId.Scp939,
            RoleTypeId.Scp049,
            RoleTypeId.Scp096,
            RoleTypeId.Scp173,
            RoleTypeId.Scp106,
        };


        [PluginEvent]
        public void OnItemUsed(PlayerUsedItemEvent ev)
        {
            if (!Check(ev.Item))
                return;

            Timing.CallDelayed(.3f, () =>
            {
                var scps = Player.GetPlayers().Where(r => r.IsSCP && r.Role != RoleTypeId.Scp079 && r.Role != RoleTypeId.Scp0492 && r.Role != RoleTypeId.Scp3114).ToList();
                if (scps.Any())
                {
                    var role = scps.RandomItem().Role;

                    _oldRoles.Add(ev.Player.UserId, ev.Player.Role);
                    ev.Player.EffectsManager.DisableEffect<Invisible>();

                    ev.Player.ChangeAppearance(role, true);
                    ev.Player.ReceiveHint($"Your appearance changed to that of {role}");

                    Timing.RunCoroutine(RestoreSkin(ev.Player, 15));
                }
                else
                {
                    var role = _scpRolestypes.RandomItem();

                    _oldRoles.Add(ev.Player.UserId, ev.Player.Role);
                    ev.Player.EffectsManager.DisableEffect<Invisible>();

                    ev.Player.ChangeAppearance(role, true);

                    Timing.RunCoroutine(RestoreSkin(ev.Player, 15));

                    // Sets hat cooldown.
                    UsableItemsController.GetHandler(ev.Player.ReferenceHub).PersonalCooldowns[ev.Item.ItemTypeId] = Time.timeSinceLevelLoad + Duration + Cooldown;

                    ev.Player.ReceiveHint($"Your appearance changed to that of {role}");
                    Timing.RunCoroutine(RestoreSkin(ev.Player, 15));
                }
            });
        }

        private IEnumerator<float> RestoreSkin(Player player, float duration)
        {
            yield return Timing.WaitForSeconds(duration);

            // round is ended.
            if (!Round.IsRoundStarted && Round.Duration.TotalSeconds > 0)
                yield break;

            if(player != null && player.IsAlive && _oldRoles.TryGetValue(player.UserId, out var role))
            {
                player.ChangeAppearance(role, false);

                _oldRoles.Remove(player.UserId);

                player.ReceiveHint($" Your appearance is back to normal.");
            }
        }
    }
}
