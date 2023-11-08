using CustomPlayerEffects;
using MEC;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Extensions;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.Ragdolls;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Respawning.RespawnEffectsController;
using UnityEngine;
using NorthwoodLib.Pools;
using NWAPI.CustomItems.API.Extensions.ScpRoles;
using Interactables.Interobjects;
using PluginAPI.Core.Attributes;
using CustomItems;

// -----------------------------------------------------------------------
// <copyright file="TranquilizerGun.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class TranquilizerGun : CustomWeapon
    {
        public static TranquilizerGun Instance;

        /// <inheritdoc/>
        public override float Damage { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.Damage;

        /// <inheritdoc/>
        public override uint Id { get; set; } = 6;

        /// <inheritdoc/>
        public override string Name { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.Name;

        /// <inheritdoc/>
        public override string Description { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.Description;

        /// <inheritdoc/>
        public override float Weight { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.Weight;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.ModelType;

        /// <inheritdoc/>
        public override byte ClipSize { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.ClipSize;

        /// <inheritdoc/>
        public override uint AttachmentsCode { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.AttachmentsCode;

        /// <inheritdoc/>
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 2,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {

                new()
                {
                   Chance = 100,
                   Location = SpawnLocationType.InsideLczArmory,
                },
                new()
                {
                   Chance = 40,
                   Location = SpawnLocationType.Inside049Armory,
                },
            },
        };

        /// <summary>
        /// Gets or sets the percent chance an SCP will resist being tranquilized. This has no effect if ResistantScps is false.
        /// </summary>
        public int ScpResistChance { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.ScpResistChance;

        /// <summary>
        /// Gets or sets the amount of time a successful tranquilization lasts for.
        /// </summary>
        public float Duration { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.Duration;

        /// <summary>
        /// Gets or sets the exponential modifier used to determine how much time is removed from the effect, everytime a player is tranquilized, they gain a resistance to further tranquilizations, reducing the duration of future effects.
        /// </summary>
        public float ResistanceModifier { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.ResistanceModifier;

        /// <summary>
        /// Gets or sets a value indicating how often player resistances are reduced.
        /// </summary>
        public float ResistanceFalloffDelay { get; set; } = EntryPoint.Instance.Config.CustomItemConfigs.TranquilizerGun.ResistanceFalloffDelay;

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

        // Fields
        private readonly Dictionary<Player, float> tranquilizedPlayers = new();
        private readonly List<Player> activeTranqs = new();
        private CoroutineHandle resistenceReducer;

        /// <inheritdoc />
        protected override void OnHurting(PlayerDamageEvent ev)
        {
            if (ev.Player is null || ev.Target is null || !Check(ev.Player.CurrentItem))
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
                }
            }

            if (ev.Target.Team == Team.SCPs)
            {
                int r = UnityEngine.Random.Range(0, 100);
                Log.Debug($"{Name}: SCP roll: {r} (must be greater than {ScpResistChance})", EntryPoint.Instance.Config.DebugMode);
                if (r <= ScpResistChance)
                {
                    Log.Debug($"{Name}: {r} is too low, no tranq.", EntryPoint.Instance.Config.DebugMode);
                    return;
                }
            }

            float duration = Duration;

            if (!tranquilizedPlayers.TryGetValue(ev.Target, out _))
                tranquilizedPlayers.Add(ev.Target, 1);

            tranquilizedPlayers[ev.Target] *= ResistanceModifier;
            Log.Debug($"{Name}: Resistance Duration Mod: {tranquilizedPlayers[ev.Target]}", EntryPoint.Instance.Config.DebugMode);

            duration -= tranquilizedPlayers[ev.Target];
            Log.Debug($"{Name}: Duration: {duration}", EntryPoint.Instance.Config.DebugMode);

            if (duration > 0f)
                Timing.RunCoroutine(DoTranquilize(ev.Target, duration));
        }

        private IEnumerator<float> DoTranquilize(Player player, float duration)
        {
            activeTranqs.Add(player);
            bool inElevator = InElevator(player);
            Vector3 oldPosition = player.Position;
            var previousItem = player.CurrentItem;
            var previousScale = player.ReferenceHub.transform.localScale;
            float newHealth = player.Health - Damage;
            List<StatusEffectBase> activeEffects = ListPool<StatusEffectBase>.Shared.Rent();
            player.CurrentItem = null;

            if (newHealth <= 0)
                yield break;

            BasicRagdoll? ragdoll = null;

            if (player.Role != RoleTypeId.Scp106)
                ragdoll = RagdollExtensions.CreateAndSpawn(player.Role, player.DisplayNickname, "Tranquilizado", player.Position, player.ReferenceHub.PlayerCameraReference.rotation, player);

            if (player.RoleBase is Scp096Role scp)
                scp.EndRage();

            try
            {
                player.EffectsManager.EnableEffect<Invisible>(duration);
                player.EffectsManager.EnableEffect<AmnesiaItems>(duration);
                player.EffectsManager.EnableEffect<AmnesiaVision>(duration);

                player.SetPlayerScale(Vector3.one * 0.02f);
                //dwplayer.Position += Vector3;
                player.Health = newHealth;
                player.IsGodModeEnabled = true;
                player.EffectsManager.EnableEffect<Ensnared>(duration);
            }
            catch (Exception e)
            {
                player.IsGodModeEnabled = false;
                Log.Error($"{nameof(TranquilizerGun)}: {e}");
            }

            yield return Timing.WaitForSeconds(duration);

            try
            {
                player.IsGodModeEnabled = false;
                ragdoll?.DestroyRagdoll();

                if (player.GameObject == null)
                    yield break;

                newHealth = player.Health;
                player.SetPlayerScale(previousScale);
                player.Health = newHealth;

                /*if (!DropItems)
                    player.CurrentItem = previousItem;*/

                /*foreach (StatusEffectBase effect in activeEffects.Where(effect => (effect.Duration - duration) > 0))
                    player.EnableEffect(effect, effect.Duration);*/

                activeTranqs.Remove(player);
                ListPool<StatusEffectBase>.Shared.Return(activeEffects);
            }
            catch (Exception e)
            {
                player.IsGodModeEnabled = false;
                Log.Error($"{nameof(DoTranquilize)}: {e}");
            }

            if (Warhead.IsDetonated && player.Position.y < 900)
            {
                player.Kill(new UniversalDamageHandler(-1f, DeathTranslations.Warhead));
                yield break;
            }

            if(!inElevator)
                player.Position = oldPosition;

        }

        private IEnumerator<float> ReduceResistances()
        {
            for (; ; )
            {
                foreach (Player player in tranquilizedPlayers.Keys)
                    tranquilizedPlayers[player] = Mathf.Max(0, tranquilizedPlayers[player] / 2);

                yield return Timing.WaitForSeconds(ResistanceFalloffDelay);
            }
        }


        /// <summary>
        /// Check if a <see cref="Player"/> is un bounds of a <see cref="ElevatorChamber"/>
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool InElevator(Player player)
        {
            foreach (var elevator in Map.Elevators)
            {
                if (elevator.WorldspaceBounds.Contains(player.Position))
                    return true;
            }

            return false;
        }

        private bool IsMoving(ElevatorChamber elevator) => elevator._curSequence is ElevatorChamber.ElevatorSequence.MovingAway or ElevatorChamber.ElevatorSequence.Arriving;
    }
}
