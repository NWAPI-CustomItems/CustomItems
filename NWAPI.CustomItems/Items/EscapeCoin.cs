using CustomPlayerEffects;
using LightContainmentZoneDecontamination;
using MEC;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Extensions;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class EscapeCoin : CustomItem
    {
        public static EscapeCoin Instance;

        /// <inheritdoc />
        public override uint Id { get; set; } = 3;

        /// <inheritdoc />
        public override string Name { get; set; } = "Escape coin";

        /// <inheritdoc />
        public override string Description { get; set; } = "Flipping this coin in the pocket dimension will you have the chance to exit at once or die instantly.";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.1f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.Coin;

        /// <inheritdoc />
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 5,
            DynamicSpawnPoints = new()
            {
                new()
                {
                    Chance = 100,
                    Location = SpawnLocationType.InsideLocker
                },
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
        public void OnCoinFlip(PlayerCoinFlipEvent ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;
            
            if(ev.Player.EffectsManager.TryGetEffect<PocketCorroding>(out var pocketCorroding) && pocketCorroding.IsEnabled)
            {
                var coin = ev.Player.CurrentItem;
                Timing.CallDelayed(3, () =>
                {
                    if (ev.IsTails)
                    {
                        if(ev.Player.IsAlive && ev.Player.RoleBase is IFpcRole role)
                        {
                            var position = Scp106PocketExitFinder.GetBestExitPosition(role);
                            ev.Player.EffectsManager.DisableEffect<PocketCorroding>();
                            ev.Player.EffectsManager.DisableEffect<Corroding>();
                            ev.Player.EffectsManager.EnableEffect<Traumatized>(40);

                            TrackedSerials.Remove(coin.ItemSerial);
                            ev.Player.RemoveItemFix(coin);
                            SpawnCoin();
                        }
                    }
                    else
                    {
                        ev.Player.Kill($"{Name}");
                    }
                });
            }
        }

        private void SpawnCoin()
        {
            Vector3 roomPosition;

            if (!DecontaminationController.Singleton.IsDecontaminating)
            {
                roomPosition = Map.GetRandomRoom(MapGeneration.FacilityZone.LightContainment).ApiRoom.Position + Vector3.up;
            }
            else if (Warhead.IsDetonated)
            {
                var randomSpawnLocation = Plugin.Random.Next(0, 4) >= 2
                    ? RoleTypeId.ChaosConscript.GetRandomSpawnLocation()
                    : RoleTypeId.NtfCaptain.GetRandomSpawnLocation();
                roomPosition = randomSpawnLocation;
            }
            else
            {
                roomPosition = Map.GetRandomRoom(MapGeneration.FacilityZone.HeavyContainment).ApiRoom.Position + Vector3.up;
            }

            Spawn(roomPosition);
        }
    }
}
