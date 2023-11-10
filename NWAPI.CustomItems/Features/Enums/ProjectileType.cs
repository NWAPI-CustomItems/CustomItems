namespace CustomItems.Features.Enums
{
    public enum ProjectileType
    {
        /// <summary>
        /// None or null projectile.
        /// </summary>
        None,

        /// <summary>
        /// Frag grenade.
        /// Used by <see cref="ItemType.GrenadeHE"/>.
        /// </summary>
        FragGrenade,

        /// <summary>
        /// Flashbang.
        /// Used by <see cref="ItemType.GrenadeFlash"/>.
        /// </summary>
        Flashbang,

        /// <summary>
        /// SCP-018 ball.
        /// Used by <see cref="ItemType.SCP018"/>.
        /// </summary>
        Scp018,

        /// <summary>
        /// SCP-2176 lightbulb.
        /// Used by <see cref="ItemType.SCP2176"/>.
        /// </summary>
        Scp2176,
    }
}
