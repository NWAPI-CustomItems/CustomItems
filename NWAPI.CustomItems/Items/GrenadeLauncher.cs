﻿using CustomItems.Features.Enums;
using CustomPlayerEffects;
using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;
using NWAPI.CustomItems.API.Extensions;
using NWAPI.CustomItems.API.Features;
using NWAPI.CustomItems.API.Features.Attributes;
using NWAPI.CustomItems.API.Spawn;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;

namespace CustomItems.Items
{
    [CustomItem]
    public class GrenadeLauncher : CustomWeapon
    {
        [YamlIgnore]
        public static GrenadeLauncher Instance;

        /// <inheritdoc/>
        [Description("Damage of the bullets, grenade damage its the same")]
        public override float Damage { get; set; } = 0;

        /// <inheritdoc/>
        public override uint Id { get; set; } = 1;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Grenade Launcher";

        /// <inheritdoc/>
        public override string Description { get; set; } = "Shot grenades and you need grenades to reloaded, all types of grenades can be used.";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1.49f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.GunLogicer;

        /// <inheritdoc/>
        public override byte ClipSize { get; set; } = 2;

        /// <inheritdoc/>
        public override uint AttachmentsCode { get; set; } = 5252;


        [Description("The grenade launchers cant use custom grenades")]
        public bool IgnoreCustomGrenade { get; set; } = true;

        /// <inheritdoc/>
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            DynamicSpawnPoints = new()
            {
                new()
                {
                    Chance = 100,
                    Location = NWAPI.CustomItems.API.Enums.SpawnLocationType.Inside079Secondary
                }
            }
        };

        /// <inheritdoc/>
        public override void SubscribeEvents()
        {
            base.SubscribeEvents();

            Instance ??= this;
            PluginAPI.Events.EventManager.RegisterEvents(EntryPoint.Instance, Instance);

        }

        /// <inheritdoc/>
        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            ClearCache();
            PluginAPI.Events.EventManager.UnregisterEvents(EntryPoint.Instance, Instance);
        }

        // Boop
        private readonly Dictionary<uint, Queue<CustomGrenade?>> LoadedCustomGrenades = new();
        private readonly Dictionary<uint, Queue<ProjectileType>> LoadedGrenades = new();
        private readonly HashSet<uint> WeaponsRealoding = new();

        /// <summary>
        /// Gets all grenades serials spawned by the grenade launcher.
        /// </summary>
        public static HashSet<ushort> GrenadesSerials = new();

        public override void Give(Player player, bool displayMessage = true)
        {
            var item = player.AddItem(ModelType);

            if (item is not Firearm firearm)
            {
                Log.Debug($"{nameof(Give)}: {Name} - {ModelType} is not a firearm", EntryPoint.Instance.Config.DebugMode);

                player.ReferenceHub.inventory.ServerRemoveItem(item.ItemSerial, null);
                return;
            }

            if (AttachmentsCode != 0)
                firearm.ChangeAttachmentsCode(AttachmentsCode);

            firearm.ChangeAmmo(ClipSize);

            if (displayMessage)
                ShowPickupMessage(player);

            TrackedSerials.Add(item.ItemSerial);

            if (!LoadedGrenades.ContainsKey(item.ItemSerial))
            {
                Queue<ProjectileType> queue = new();

                for (int i = 0; i < ClipSize; i++)
                {
                    queue.Enqueue(ProjectileType.FragGrenade);
                }

                LoadedGrenades.Add(item.ItemSerial, queue);
            }

            if (!LoadedCustomGrenades.ContainsKey(item.ItemSerial))
            {
                Queue<CustomGrenade> queue = new();
                LoadedCustomGrenades.Add(item.ItemSerial, queue);
            }
        }

        [PluginEvent]
        public void OnWaitingForPlayers(WaitingForPlayersEvent _)
        {
            ClearCache();
        }

        public override void OnPickedup(PlayerSearchedPickupEvent ev)
        {
            base.OnPickedup(ev);

            if (!Check(ev.Item))
                return;

            Log.Debug($"{ev.Player.LogName} is pickup {Name}", EntryPoint.Instance.Config.DebugMode);
            if (!LoadedGrenades.ContainsKey(ev.Item.NetworkInfo.Serial))
            {
                Queue<ProjectileType> queue = new();

                for (int i = 0; i < ClipSize; i++)
                {
                    queue.Enqueue(ProjectileType.FragGrenade);
                }

                LoadedGrenades.Add(ev.Item.NetworkInfo.Serial, queue);
            }

            if (!LoadedCustomGrenades.ContainsKey(ev.Item.NetworkInfo.Serial))
            {
                Queue<CustomGrenade> queue = new();
                LoadedCustomGrenades.Add(ev.Item.NetworkInfo.Serial, queue);
            }
        }

        protected override bool OnReloading(PlayerReloadWeaponEvent ev)
        {
            if (!Check(ev.Firearm.ItemSerial))
                return true;

            if (ev.Player.CurrentItem is not Firearm firearm || firearm.Status.Ammo >= ClipSize || WeaponsRealoding.Contains(ev.Firearm.ItemSerial))
                return false;

            Log.Debug($"{ev.Player.LogName} is reloading {Name}", EntryPoint.Instance.Config.DebugMode);
            bool isCustomGrenade = false;

            foreach (var item in ev.Player.Items.ToList())
            {
                if (!item.ItemTypeId.IsThrowable())
                    continue;

                if (TryGet(item, out CustomItem? customItem))
                {
                    if (IgnoreCustomGrenade)
                        continue;

                    if (customItem is CustomGrenade customGrenade)
                    {
                        LoadedCustomGrenades[ev.Firearm.ItemSerial].Enqueue(customGrenade);
                        isCustomGrenade = true;
                    }
                }

                ev.Player.EffectsManager.DisableEffect<Invisible>();
                WeaponsRealoding.Add(ev.Firearm.ItemSerial);

                ev.Player.Connection.Send(new RequestMessage(ev.Firearm.ItemSerial, RequestType.Reload));

                Timing.CallDelayed(1.5f, () =>
                {
                    var newAmmo = (byte)(ev.Firearm.Status.Ammo + 1);

                    if (newAmmo < 0)
                        newAmmo = 0;

                    if (newAmmo > ClipSize)
                        newAmmo = ClipSize;

                    FirearmStatusFlags flags = firearm.Status.Flags;
                    if ((flags & FirearmStatusFlags.MagazineInserted) == 0)
                    {
                        flags |= FirearmStatusFlags.MagazineInserted;
                    }

                    firearm.Status = new FirearmStatus(newAmmo, flags, firearm.Status.Attachments);

                    WeaponsRealoding.Remove(ev.Firearm.ItemSerial);
                });

                if (isCustomGrenade)
                {
                    Log.Debug($"{Name}.{nameof(OnReloading)}: {ev.Player.Nickname} successfully reloaded. Grenade type: {item.ItemTypeId} IsCustomGrenade: {isCustomGrenade}",
                    EntryPoint.Instance.Config.DebugMode);

                    ev.Player.ReferenceHub.inventory.ServerRemoveItem(item.ItemSerial, null);
                    break;
                }

                ProjectileType type = item.ItemTypeId switch
                {
                    ItemType.GrenadeFlash => ProjectileType.Flashbang,
                    ItemType.GrenadeHE => ProjectileType.FragGrenade,
                    ItemType.SCP018 => ProjectileType.Scp018,
                    ItemType.SCP2176 => ProjectileType.Scp2176,
                    _ => ProjectileType.FragGrenade
                };
                LoadedGrenades[ev.Firearm.ItemSerial].Enqueue(type);
                Log.Debug($"{Name}.{nameof(OnReloading)}: {ev.Player.Nickname} successfully reloaded. Grenade type: {type} IsCustomGrenade: {isCustomGrenade}",
                EntryPoint.Instance.Config.DebugMode);

                ev.Player.ReferenceHub.inventory.ServerRemoveItem(item.ItemSerial, null);
                break;
            }

            return false;
        }

        protected override void OnShoot(PlayerShotWeaponEvent ev)
        {
            if (!Check(ev.Firearm.ItemSerial))
                return;

            if (ev.Player.CurrentItem is not Firearm firearm || firearm.Status.Ammo < 0 ||
            firearm.Status.Ammo > firearm.AmmoManagerModule.MaxAmmo)
                return;

            Log.Debug($"{ev.Player.LogName} is shooting {Name}", EntryPoint.Instance.Config.DebugMode);

            if (LoadedGrenades.TryGetValue(ev.Firearm.ItemSerial, out var queue) && queue.TryDequeue(out var grenade))
            {
                ThrowGrenade(ev.Player, grenade);
                return;
            }

            if (LoadedCustomGrenades.TryGetValue(ev.Firearm.ItemSerial, out var customGrenadesQueue) && customGrenadesQueue.TryDequeue(out var customGrenade))
            {
                Log.Debug($"{ev.Player.LogName} is shooting {Name} and is a CustomGrenade {customGrenade?.Name}", EntryPoint.Instance.Config.DebugMode);
                customGrenade?.Throw(ev.Player.Position, true, customGrenade.Weight, customGrenade.ModelType, ev.Player);
                return;
            }

            Log.Warning($"{Name}.{nameof(OnShoot)}: {ev.Player.Nickname} had a null grenade");
        }

        private void ThrowGrenade(Player player, ProjectileType type)
        {
            Log.Debug($"ThrowGrenade invoked", EntryPoint.Instance.Config.DebugMode);
            ThrowableItem? throwable = type switch
            {
                ProjectileType.Flashbang => ItemType.GrenadeFlash.CreateThrowableItem(player),
                ProjectileType.Scp2176 => ItemType.SCP2176.CreateThrowableItem(player),
                ProjectileType.Scp018 => ItemType.SCP018.CreateThrowableItem(player),
                _ => ItemType.GrenadeHE.CreateThrowableItem(player)
            };

            if (throwable == null)
                return;

            GrenadesSerials.Add(throwable.ItemSerial);

            throwable.Owner = player.ReferenceHub;
            Log.Debug($"{player.LogName} is throwing a grenade from {Name}", EntryPoint.Instance.Config.DebugMode);
            player.ThrowItem(throwable, true);
        }

        private void ClearCache()
        {
            LoadedGrenades.Clear();
            WeaponsRealoding.Clear();
            LoadedCustomGrenades.Clear();
            GrenadesSerials.Clear();
            TrackedSerials.Clear();
        }
    }
}
