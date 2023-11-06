using CustomItems.Features.Enums;
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
using System.Linq;

namespace CustomItems.Items
{
    [CustomItem]
    public class GrenadeLauncher : CustomWeapon
    {

        public static GrenadeLauncher Instance;

        /// <inheritdoc/>
        public override float Damage { get; set; } = 0;

        /// <inheritdoc/>
        public override uint Id { get; set; } = 1;

        /// <inheritdoc/>
        public override string Name { get; set; } = "GL";

        /// <inheritdoc/>
        public override string Description { get; set; } = "Shot grenades, your welcome.";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 1f;

        /// <inheritdoc/>
        public override ItemType ModelType { get; set; } = ItemType.GunLogicer;

        /// <inheritdoc/>
        public override byte ClipSize { get; set; } = 4;

        /// <inheritdoc/>
        public override uint AttachmentsCode { get; set; } = 5252;

        public bool IgnoreCustomGrenade { get; set; } = true;


        public static HashSet<ushort> GrenadesSerials = new();

        /// <inheritdoc/>
        public override SpawnProperties? SpawnProperties { get; set; } = new()
        {
            DynamicSpawnPoints = new()
            {
                new()
                {
                    Chance = 100,
                    Location = NWAPI.CustomItems.API.Enums.SpawnLocationType.Inside914
                }
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
            ClearCache();
            PluginAPI.Events.EventManager.UnregisterEvents(Plugin.Instance, Instance);
        }

        // Boop
        private readonly Dictionary<uint, Queue<CustomGrenade?>> LoadedCustomGrenades = new();
        private readonly Dictionary<uint, Queue<ProjectileType>> LoadedGrenades = new();
        private readonly HashSet<uint> WeaponsRealoding = new();

        [PluginEvent]
        public void OnRoundEnd(RoundEndEvent _)
        {
            ClearCache();
        }

        public override void OnPickedup(PlayerSearchedPickupEvent ev)
        {
            base.OnPickedup(ev);

            if (!LoadedGrenades.ContainsKey(ev.Item.NetworkInfo.Serial))
            {
                Log.Info($" LoadedGrenades {LoadedCustomGrenades.ContainsKey(ev.Item.NetworkInfo.Serial)}");
                Queue<ProjectileType> queue = new();

                for (int i = 0; i < ClipSize; i++)
                {
                    queue.Enqueue(ProjectileType.FragGrenade);
                }

                LoadedGrenades.Add(ev.Item.NetworkInfo.Serial, queue);
            }

            if (!LoadedCustomGrenades.ContainsKey(ev.Item.NetworkInfo.Serial))
            {
                Log.Info($"LoadedCustomGrenades {LoadedCustomGrenades.ContainsKey(ev.Item.NetworkInfo.Serial)}");
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

            bool isCustomGrenade = false;

            foreach(var item in ev.Player.Items.ToList())
            {
                if (!item.ItemTypeId.IsThrowable())
                    continue;

                if (TryGet(item, out CustomItem? customItem))
                {
                    if (IgnoreCustomGrenade)
                        continue;

                    if(customItem is CustomGrenade customGrenade)
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

                ProjectileType type = item.ItemTypeId switch
                {
                    ItemType.GrenadeFlash => ProjectileType.Flashbang,
                    ItemType.GrenadeHE => ProjectileType.FragGrenade,
                    ItemType.SCP018 => ProjectileType.Scp018,
                    ItemType.SCP2176 => ProjectileType.Scp2176,
                    _ => ProjectileType.FragGrenade
                };
                LoadedGrenades[ev.Firearm.ItemSerial].Enqueue(type);
                Log.Debug($"{Name}.{nameof(OnReloading)}: {ev.Player.Nickname} successfully reloaded. Grenade type: {type} IsCustom: {isCustomGrenade}",
                Plugin.Instance.Config.DebugMode);

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

            if (LoadedGrenades.TryGetValue(ev.Firearm.ItemSerial, out var queue) && queue.TryDequeue(out var grenade))
            {
                ThrowGrenade(ev.Player, grenade);
                return;
            }

            if(LoadedCustomGrenades.TryGetValue(ev.Firearm.ItemSerial, out var customGrenadesQueue) && customGrenadesQueue.TryDequeue(out var customGrenade))
            {
                customGrenade?.Throw(ev.Player.Position, true, customGrenade.Weight, customGrenade.ModelType, ev.Player);
                return;
            }

            Log.Warning($"{Name}.{nameof(OnShoot)}: {ev.Player.Nickname} had a null grenade");
        }

        private void ThrowGrenade(Player player, ProjectileType type)
        {
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
