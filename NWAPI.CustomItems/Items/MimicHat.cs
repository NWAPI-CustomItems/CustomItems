using CustomItems;
using CustomPlayerEffects;
using InventorySystem.Items.Usables;
using MEC;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Extensions;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class MimicHat : CustomItem
    {
        [YamlIgnore]
        public static MimicHat Instance;

        /// <inheritdoc />
        public override uint Id { get; set; } = 7;

        /// <inheritdoc />
        public override string Name { get; set; } = "Mimic hat";

        /// <inheritdoc />
        public override string Description { get; set; } = "Wearing this hat will change your appearance to that of a random live SCP for a period of time.";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.1f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.SCP268;

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

        /// <summary>
        /// Gets or sets the disguise duration.
        /// </summary>
        [Description("Disguise duration")]
        public float Duration { get; set; } = 15f;

        /// <summary>
        /// Gets or sets the cooldown when the item is used.
        /// </summary>
        [Description("Cooldown after using the custom item the cooldown is calculated with (duration + cooldown)")]
        public float Cooldown { get; set; } = 40f;

        /// <summary>
        /// Gets or sets the hint message when a player appearance is changed.
        /// </summary>
        [Description("Hint that will appear on a player's face when changing appearance. {0} is the appearance the player took (roletype).")]
        public string AppearanceChange { get; set; } = "Your appearance changed to that of {0}";

        /// <summary>
        /// Gets or sets hint message when the appearance is restored to normal.
        /// </summary>
        [Description("Hint that will appear when the player's appearance returns to normal.")]
        public string AppearanceRestore { get; set; } = "Your appearance is back to normal.";

        /// <summary>
        /// Gets or sets the hint message duration.
        /// </summary>
        [Description("The hint message of the disguise.")]
        public float HintDuration { get; set; } = 5f;

        /// <summary>
        /// Gets or sets if the custom item will be spawned.
        /// </summary>
        [Description("If this is true, this custom item will not spawn.")]
        public bool IsDisabled { get; set; } = false;

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

        private readonly Dictionary<string, RoleTypeId> _oldRoles = new();
        private readonly List<RoleTypeId> _scpRolestypes = new()
        {
            RoleTypeId.Scp939,
            RoleTypeId.Scp049,
            RoleTypeId.Scp096,
            RoleTypeId.Scp173,
            RoleTypeId.Scp106,
        };

        public override void SpawnAll()
        {
            if (IsDisabled)
                return;

            base.SpawnAll();
        }

        [PluginEvent]
        public void OnWaitingForPlayer(WaitingForPlayersEvent _)
        {
            _oldRoles.Clear();
        }

        [PluginEvent]
        public void OnItemUsed(PlayerUsedItemEvent ev)
        {
            if (!Check(ev.Item))
                return;

            Log.Debug($"{ev.Player.LogName} is using {Name}", EntryPoint.Instance.Config.DebugMode);

            Timing.CallDelayed(.5f, () =>
            {
                var scps = Player.GetPlayers().Where(r => r.IsSCP && r.Role != RoleTypeId.Scp079 && r.Role != RoleTypeId.Scp0492 && r.Role != RoleTypeId.Scp3114).ToList();
                if (scps.Any())
                {
                    var role = scps.RandomItem().Role;

                    _oldRoles.Add(ev.Player.UserId, ev.Player.Role);
                    ev.Player.EffectsManager.DisableEffect<Invisible>();

                    ev.Player.ChangeAppearance(role, true);
                    ev.Player.ReceiveHint(string.Format(AppearanceChange, role), HintDuration);

                    // Sets hat cooldown.
                    UsableItemsController.GetHandler(ev.Player.ReferenceHub).PersonalCooldowns[ev.Item.ItemTypeId] = Time.timeSinceLevelLoad + Duration + Cooldown;

                    Timing.RunCoroutine(RestoreSkin(ev.Player, Duration));
                }
                else
                {
                    var role = _scpRolestypes.RandomItem();

                    _oldRoles.Add(ev.Player.UserId, ev.Player.Role);
                    ev.Player.EffectsManager.DisableEffect<Invisible>();

                    ev.Player.ChangeAppearance(role, true);

                    Timing.RunCoroutine(RestoreSkin(ev.Player, Duration));

                    // Sets hat cooldown.
                    UsableItemsController.GetHandler(ev.Player.ReferenceHub).PersonalCooldowns[ev.Item.ItemTypeId] = Time.timeSinceLevelLoad + Duration + Cooldown;

                    ev.Player.ReceiveHint(string.Format(AppearanceChange, role), HintDuration);
                    Timing.RunCoroutine(RestoreSkin(ev.Player, Duration));
                }
            });
        }

        private IEnumerator<float> RestoreSkin(Player player, float duration)
        {
            Log.Debug($"{player.LogName} restore skin invoked waiting {duration} | {Name}", EntryPoint.Instance.Config.DebugMode);
            yield return Timing.WaitForSeconds(duration);

            // round is ended.
            if (!Round.IsRoundStarted && Round.Duration.TotalSeconds > 0)
                yield break;

            if (player != null && player.IsReady && player.IsAlive && _oldRoles.TryGetValue(player.UserId, out var role))
            {
                Log.Debug($"{player.LogName} restoring skin with the item {Name}", EntryPoint.Instance.Config.DebugMode);
                player.ChangeAppearance(role, false);

                _oldRoles.Remove(player.UserId);

                player.ReceiveHint(AppearanceRestore, HintDuration);
            }
        }
    }
}
