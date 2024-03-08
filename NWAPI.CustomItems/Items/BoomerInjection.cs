using CustomItems;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using NWAPI.CustomItems.Commands;
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
using YamlDotNet.Serialization;
using static PlayerList;
using UnityEngine;
using NWAPI.CustomItems.API.Extensions;
using MapGeneration;
using MEC;
using System.ComponentModel;
using InventorySystem.Items.ThrowableProjectiles;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class BoomerInjection : CustomItem
    {
        [YamlIgnore]
        public static BoomerInjection Instance;

        /// <inheritdoc />
        public override uint Id { get; set; } = 12;

        /// <inheritdoc />
        public override string Name { get; set; } = "Boomer Injection";

        /// <inheritdoc />
        public override string Description { get; set; } = "By using the injection you will be infected with a special strain of SCP-008 that will cause you to explode when damaged and leave poison in the room you are in for a period of time.";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.1f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.Adrenaline;

        /// <inheritdoc />
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 2,
            DynamicSpawnPoints = new()
            {
                new()
                {
                    Chance = 25,
                    Location = SpawnLocationType.Inside330Chamber,
                },
                new()
                {
                    Chance = 25,
                    Location = SpawnLocationType.Inside049Armory,
                },
            }
        };

        [Description("The reason for the death the player will see when exploding as a boomer")]
        public string BoomerKillReason { get; set; } = "Boomer boom";

        [Description("The time that a room will remain with the poisonous gas")]
        public int GasDuration { get; set; } = 20;

        [Description("The amount of damage the poison will do per second.")]
        public int GasDamage { get; set; } = 5;

        [Description("The reason for the damage done by the poison, if a player dies from the poison he will see this on the death screen")]
        public string GasDamageReason { get; set; } = "Boomer poisoned room";

        [Description("The message someone will get when they enter a room with poison for the first time.")]
        public string WarningHint { get; set; } = "Something smells bad...";

        [Description("The hint message that the player will be shown when using the item")]
        public string InfectedHint { get; set; } = "You feel bloated";

        [Description("The duration of the hints on the screen")]
        public float HintDuration { get; set; } = 6f;

        [Description("The room when infected its lights will change color, if the color is equal to white (255, 255, 255) the color of the room will not change.")]
        public Color GasRoomColor { get; set; } = Color.green;
        


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

        private List<string> BoomerPlayers = new();

        [PluginEvent]
        public void OnItemUsed(PlayerUsedItemEvent ev)
        {
            if (!Check(ev.Item))
                return;

            Log.Debug($"{ev.Player.LogName} is using {Name}", EntryPoint.Instance.Config.DebugMode);

            BoomerPlayers.Add(ev.Player.UserId);

            ev.Player.ReceiveHint(InfectedHint, HintDuration);
        }

        [PluginEvent]
        public void OnPlayerDamage(PlayerDamageEvent ev)
        {
            try
            {
                if (ev.Player is null || ev.Player.IsServer || ev.DamageHandler is ExplosionDamageHandler || !BoomerPlayers.Contains(ev.Target.UserId))
                    return;

                if (ev.Target.Room is null || ev.Target.Room.Name == RoomName.Outside)
                    return;

                BoomerPlayers.Remove(ev.Target.UserId); ;

                if (ev.Target.Room.gameObject.TryGetComponent<BoomerPoisonedRoom>(out var oldcomp))
                    oldcomp.Disable();

                var component = ev.Target.Room.gameObject.AddComponent<BoomerPoisonedRoom>();
                SpawnExplosive(ev.Target, ItemType.GrenadeHE, 0.1f);
                component.Init(GasDuration, GasDamage, GasDamageReason, GasRoomColor, WarningHint, HintDuration);

                ev.Target.Kill(BoomerKillReason);
            }
            catch (Exception e)
            {
                Log.Error($"Error on {nameof(OnPlayerDamage)} on CustomItem {Name}: {e.Message}");
            }
        }

        [PluginEvent]
        public void OnPlayerDeath(PlayerDeathEvent ev)
        {
            if (!BoomerPlayers.Contains(ev.Player.UserId))
                return;

            BoomerPlayers.Remove(ev.Player.UserId);
        }

        [PluginEvent]
        public void OnMapGenerated(MapGeneratedEvent _)
        {
            BoomerPlayers.Clear();
        }

        /// <summary>
        /// Spawns and activates an explosive throwable item of the specified ItemType.
        /// </summary>
        /// <param name="ply">The player triggering the spawn.</param>
        /// <param name="type">The ItemType of the explosive to spawn.</param>
        /// <param name="fuse">The fuse time for the explosive (default is 1 second).</param>
        /// <param name="playerIsOwner">Determines if the player is considered the owner of the explosive (default is false).</param>
        private void SpawnExplosive(Player ply, ItemType type, float fuse = 1f, bool playerIsOwner = false)
        {
            try
            {
                // Check if the ItemType is throwable
                if (!type.IsThrowable())
                {
                    return;
                }
                // Create a throwable item of the specified ItemType
                ThrowableItem? throwableItem = type.CreateThrowableItem();

                // Spawn and activate the throwable item at the player's position
                if (throwableItem != null)
                {
                    // Determine the owner of the explosive based on the playerIsOwner parameter
                    Player? owner = playerIsOwner ? ply : null;

                    // Spawn and activate the explosive
                    throwableItem.SpawnAndActivateThrowable(ply.Position, fuse, owner);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error on {e}");
            }
        }

    }

    internal class BoomerPoisonedRoom : MonoBehaviour
    {
        /// <summary>
        /// Gets the room who has the poison gas.
        /// </summary>
        public RoomIdentifier Room { get; private set; } = null!;

        /// <summary>
        /// Gets the player who is inside the room.
        /// </summary>
        private List<Player> _playersInRoom = new();

        /// <summary>
        /// Gets the coroutine handle who is in charge of damaging players
        /// </summary>
        private CoroutineHandle _handler;

        /// <summary>
        /// Gets the duration of the gas in the room.
        /// </summary>
        private int GasRoomDuration = 0;

        /// <summary>
        /// Gets the amount of damage done by poison gas.
        /// </summary>
        private float GasDamagePerSecondTick = 0;

        /// <summary>
        /// Gets the damage reason done by posion gas.
        /// </summary>
        private string DamageReason = "";

        /// <summary>
        /// Gets the hint message to send to the players when touche the room for the first time
        /// </summary>
        private string WarningHint = "";

        /// <summary>
        /// Gets the hint duration.
        /// </summary>
        private float HintDuration = 5f;

        /// <summary>
        /// Gets the default light color of the room, used for restarting the lights to its original color.
        /// </summary>
        private Color RoomOriginalColor;

        private float _updateInterval = 0.9f;
        private float _timeSinceLastUpdate = 0f;
        private RoomLightController Lights = null!;
        private List<string> _playersWarned = new();
        

        /// <summary>
        /// Gets cached player userid
        /// </summary>
        private string userId = string.Empty;

        private bool _initialized = false;

        private void Start()
        {
            Room = GetComponent<RoomIdentifier>();

            if(Room == null)
            {
                Disable();

                Log.Warning($"Room is null in Start method in BoomerInjection.GasRoom");
                return;
            }

            Lights = Room.GetComponentInChildren<RoomLightController>();

            if (Lights is null)
            {
                Disable();
                Log.Warning($"Lights is null in Start method in BoomerInjection.GasRoom");
                return;
            }
            RoomOriginalColor = Lights.NetworkOverrideColor;
            _initialized = true;
        }

        private void OnDestroy()
        {
            if (Room != null && Lights != null)
            {
                Lights.NetworkOverrideColor = RoomOriginalColor;
                Lights.ServerFlickerLights(1);
            }

            _playersWarned.Clear();
            _playersInRoom.Clear();

            Timing.KillCoroutines(_handler);
        }

        private void Update()
        {
            _timeSinceLastUpdate += Time.deltaTime;

            if (_timeSinceLastUpdate >= _updateInterval)
            {
                UpdatePlayersInRoom();
                _timeSinceLastUpdate = 0f;
            }
        }

        private void UpdatePlayersInRoom()
        {
            // Clears players in room
            _playersInRoom.Clear();

            // Obtains players all players alive.
            var players = Player.GetPlayers().Where(p => p.IsAlive);

            foreach (var player in players)
            {
                if (player != null && player.Room != null && player.Room.transform == Room.transform)
                {
                    _playersInRoom.Add(player);
                }
            }
        }

        public void Disable()
        {
            Destroy(this);
        }

        private bool IsStarted() => _initialized;

        /// <summary>
        /// Starts the coroutine to damage players in the room and override the color
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="damage"></param>
        /// <param name="reason"></param>
        /// <param name="color"></param>
        public void Init(int duration, float damage, string reason, Color color, string warningHint, float hintDuration)
        {
            GasRoomDuration = duration;
            GasDamagePerSecondTick = damage;
            DamageReason = reason;
            WarningHint = warningHint;
            HintDuration = hintDuration;

            Timing.CallDelayed(Timing.WaitUntilTrue(IsStarted), () =>
            {
                if (color != Color.white)
                    Lights.NetworkOverrideColor = color;

                _handler = Timing.RunCoroutine(DamagePlayer());
            });
        }

        private IEnumerator<float> DamagePlayer()
        {
            while (GasRoomDuration > 0)
            {
                if (!Lights.NetworkLightsEnabled)
                    Lights.NetworkLightsEnabled = true;

                try
                {
                    if (_playersInRoom.Count > 0)
                    {
                        foreach (var player in _playersInRoom)
                        {
                            if(!_playersWarned.Contains(player.UserId))
                            {
                                player.ReceiveHint(WarningHint, HintDuration);
                                _playersWarned.Add(player.UserId);
                            }

                            player.Damage(GasDamagePerSecondTick, DamageReason);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Log.Error($"Error on {nameof(BoomerPoisonedRoom)}::{nameof(DamagePlayer)}: {e} || {e.GetType()}");
                }

                GasRoomDuration--;
                yield return Timing.WaitForSeconds(1);
            }

            // Destroys the component when the duration is reached.
            Disable();
        }
    }
}
