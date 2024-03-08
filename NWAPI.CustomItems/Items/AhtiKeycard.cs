using GameCore;
using Interactables.Interobjects.DoorUtils;
using MEC;
using NWAPI.CustomItems.API.Enums;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Spawn;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Serialization;
using static PlayerList;

namespace NWAPI.CustomItems.Items
{
    [API.Features.Attributes.CustomItem]
    public class AhtiKeycard : CustomItem
    {
        [YamlIgnore]
        public static AhtiKeycard Instance;

        /// <inheritdoc />
        public override uint Id { get; set; } = 11;

        /// <inheritdoc />
        public override string Name { get; set; } = "Ahti Keycard"; // Control my beloved

        /// <inheritdoc />
        public override string Description { get; set; } = "You can open any door with this keycard at cost of your health";

        /// <inheritdoc />
        public override float Weight { get; set; } = 0.2f;

        /// <inheritdoc />
        public override ItemType ModelType { get; set; } = ItemType.KeycardJanitor;

        /// <inheritdoc />
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            DynamicSpawnPoints = new()
            {
                new()
                {
                    Chance = 10,
                    Location = SpawnLocationType.InsideLczCafe,
                },
            }
        };

        [Description("Cost of health for any listed door")]
        public Dictionary<string, float> HealthCosts { get; set; } = new()
        {
            { "079_SECOND", 110 }, // Yes this kill the player HAHA!
            { "079_FIRST", 110 },
            { "HID", 80 },
            { "GateA", 40 },
            { "GateB", 40 },
        };

        [Description("If any of the open doors are listed here, they will close after the specified time.")]
        public Dictionary<string, float> TemporaryOpenedDoors { get; set; } = new()
        {
            { "079_SECOND", 40f }, // Yes this kill the player HAHA!
            { "079_FIRST", 10f },
            { "HID", 80f },
        };

        [Description("If the door is not listed in HealthCosts this value will be used")]
        public float DefaultHealthCost { get; set; } = 30f;

        [Description("Doors that can't be opened with this custom keycard")]
        public List<string> BlacklistedDoors { get; set; } = new()
        {
            "SomeDoor",
        };

        [Description("Damage reason")]
        public string Reason { get; set; } = "Damaged by Ahti Keycard";

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

        private List<string> _delayedClosedDoors = new();

        [PluginEvent]
        public bool OnPlayerInteractDoorEvent(PlayerInteractDoorEvent ev)
        {
            if (ev.CanOpen || ev.Door.NetworkActiveLocks > 0 && ev.Player.IsBypassEnabled)
                return true;

            var doorName = GetDoorName(ev.Door);

            if (!Check(ev.Player.CurrentItem) ||BlacklistedDoors.Contains(doorName))
                return true;

            if (HealthCosts.TryGetValue(doorName, out float healthCosts))
                ev.Player.Damage(healthCosts, Reason);
            else
                ev.Player.Damage(DefaultHealthCost, Reason);


            ToggleDoor(ev.Door, ev.Player.ReferenceHub);

            if (TemporaryOpenedDoors.TryGetValue(doorName, out var delayClose) && !_delayedClosedDoors.Contains(doorName))
                Timing.RunCoroutine(DelayedClose(ev.Door, delayClose, doorName));

            PluginAPI.Core.Log.Debug($"{ev.Player.LogName} opened {doorName} at cost of {(healthCosts != 0 ? healthCosts : DefaultHealthCost)}", Plugin.Instance.Config.DebugMode);
            return false;
        }

        [PluginEvent]
        public void OnMapGenerated(MapGeneratedEvent _)
        {
            _delayedClosedDoors.Clear();
        }

        private string GetDoorName(DoorVariant door)
        {
            var nametag = door.GetComponent<DoorNametagExtension>();
            GetBefore(door.gameObject.name, ' ');

            return nametag == null ? GetBefore(door.gameObject.name, ' ') : RemoveBracketsOnEndOfName(nametag.GetName);
        }

        private void ToggleDoor(DoorVariant dr, ReferenceHub hub)
        {
            dr.NetworkTargetState = !dr.NetworkTargetState;
            dr._triggerPlayer = hub;

            switch (dr.NetworkTargetState)
            {
                case false:
                    DoorEvents.TriggerAction(dr, DoorAction.Closed, hub);
                    break;
                case true:
                    DoorEvents.TriggerAction(dr, DoorAction.Opened, hub);
                    break;
            }
        }

        private IEnumerator<float> DelayedClose(DoorVariant door, float delay, string doorname)
        {
            _delayedClosedDoors.Add(doorname);
            yield return Timing.WaitForSeconds(delay);

            door.NetworkTargetState = false;
            _delayedClosedDoors.Remove(doorname);
        }

        // -----------------------------------------------------------------------
        // <copyright file="StringExtensions.cs" company="Exiled Team">
        // Copyright (c) Exiled Team. All rights reserved.
        // Licensed under the CC BY-SA 3.0 license.
        // </copyright>
        // -----------------------------------------------------------------------

        /// <summary>
        /// Retrieves a string before a symbol from an input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="symbol">The symbol.</param>
        /// <returns>Substring before the symbol.</returns>
        public string GetBefore(string input, char symbol)
        {
            int start = input.IndexOf(symbol);
            if (start > 0)
                input = input.Substring(0, start);

            return input;
        }

        /// <summary>
        /// Removes the prefab-generated brackets (#) on <see cref="UnityEngine.GameObject"/> names.
        /// </summary>
        /// <param name="name">Name of the <see cref="UnityEngine.GameObject"/>.</param>
        /// <returns>Name without brackets.</returns>
        public string RemoveBracketsOnEndOfName(string name)
        {
            int bracketStart = name.IndexOf('(') - 1;

            if (bracketStart > 0)
                name = name.Remove(bracketStart, name.Length - bracketStart);

            return name;
        }

        // Thanks exiled <3
    }
}
